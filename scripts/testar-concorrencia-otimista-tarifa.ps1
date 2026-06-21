$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$baseUrl = "http://localhost:5196"
$idTarifa = 1

Write-Host "=== TESTE DE CONCORRENCIA OTIMISTA - TARIFA ==="

try {
    $clienteA = Invoke-RestMethod -Uri "$baseUrl/api/Tarifas/$idTarifa" -Method GET
    $clienteB = Invoke-RestMethod -Uri "$baseUrl/api/Tarifas/$idTarifa" -Method GET
}
catch {
    Write-Host "ERRO: Nao foi possivel buscar a tarifa."
    Write-Host $_.Exception.Message
    exit
}

Write-Host "Funcionario A leu RowVersion:" $clienteA.rowVersion
Write-Host "Funcionario B leu RowVersion:" $clienteB.rowVersion

$bodyA = @{
    idTarifa = $clienteA.idTarifa
    tipoTarifa = $clienteA.tipoTarifa
    valorPrimeiraHora = $clienteA.valorPrimeiraHora + 1
    valorHoraAdicional = $clienteA.valorHoraAdicional
    valorDiaria = $clienteA.valorDiaria
    descontoParceiro = $clienteA.descontoParceiro
    descontoFuncionario = $clienteA.descontoFuncionario
    rowVersion = $clienteA.rowVersion
} | ConvertTo-Json

Write-Host "`nFuncionario A tentando atualizar..."

try {
    Invoke-RestMethod `
        -Uri "$baseUrl/api/Tarifas/$idTarifa" `
        -Method PUT `
        -ContentType "application/json" `
        -Body $bodyA

    Write-Host "SUCESSO: Funcionario A atualizou com sucesso."
}
catch {
    Write-Host "ERRO: Funcionario A falhou inesperadamente."
    Write-Host $_.Exception.Message
    exit
}

$bodyB = @{
    idTarifa = $clienteB.idTarifa
    tipoTarifa = $clienteB.tipoTarifa
    valorPrimeiraHora = $clienteB.valorPrimeiraHora + 2
    valorHoraAdicional = $clienteB.valorHoraAdicional
    valorDiaria = $clienteB.valorDiaria
    descontoParceiro = $clienteB.descontoParceiro
    descontoFuncionario = $clienteB.descontoFuncionario
    rowVersion = $clienteB.rowVersion
} | ConvertTo-Json

Write-Host "`nFuncionario B tentando atualizar com RowVersion antiga..."

try {
    Invoke-RestMethod `
        -Uri "$baseUrl/api/Tarifas/$idTarifa" `
        -Method PUT `
        -ContentType "application/json" `
        -Body $bodyB

    Write-Host "ERRO: Funcionario B conseguiu atualizar, mas deveria receber conflito."
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__

    if ($statusCode -eq 409) {
        Write-Host "SUCESSO: Funcionario B recebeu 409 Conflict."
        Write-Host "A concorrencia otimista de tarifas esta funcionando."
    }
    else {
        Write-Host "ERRO: Funcionario B falhou, mas com outro status:" $statusCode
        Write-Host $_.Exception.Message
    }
}