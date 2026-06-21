$ErrorActionPreference = "Stop"
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

$baseUrl = "http://localhost:5196"
$idUsuario = 1

Write-Host "=== TESTE DE CONCORRENCIA OTIMISTA - USUARIO ==="

try {
    $clienteA = Invoke-RestMethod -Uri "$baseUrl/api/Usuarios/$idUsuario" -Method GET
    $clienteB = Invoke-RestMethod -Uri "$baseUrl/api/Usuarios/$idUsuario" -Method GET
}
catch {
    Write-Host "ERRO: Nao foi possivel buscar o usuario."
    Write-Host $_.Exception.Message
    exit
}

Write-Host "Funcionario A leu RowVersion:" $clienteA.rowVersion
Write-Host "Funcionario B leu RowVersion:" $clienteB.rowVersion

if ($null -eq $clienteA.rowVersion -or $null -eq $clienteB.rowVersion) {
    Write-Host "ERRO: A RowVersion nao veio na resposta da API."
    exit
}

# Funcionário A atualiza primeiro
$bodyA = @{
    idUsuario = $clienteA.idUsuario
    tipoUsuario = $clienteA.tipoUsuario
    cpf = $clienteA.cpf
    nome = $clienteA.nome
    dataNascimento = $clienteA.dataNascimento
    cargo = "Gerente Teste A"
    telefone = $clienteA.telefone
    endereco = $clienteA.endereco
    senhaAcesso = $clienteA.senhaAcesso
    rowVersion = $clienteA.rowVersion
} | ConvertTo-Json

Write-Host "`nFuncionario A tentando atualizar..."

try {
    Invoke-RestMethod `
        -Uri "$baseUrl/api/Usuarios/$idUsuario" `
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

# Funcionário B tenta atualizar usando a RowVersion antiga
$bodyB = @{
    idUsuario = $clienteB.idUsuario
    tipoUsuario = $clienteB.tipoUsuario
    cpf = $clienteB.cpf
    nome = $clienteB.nome
    dataNascimento = $clienteB.dataNascimento
    cargo = "Gerente Teste B"
    telefone = $clienteB.telefone
    endereco = $clienteB.endereco
    senhaAcesso = $clienteB.senhaAcesso
    rowVersion = $clienteB.rowVersion
} | ConvertTo-Json

Write-Host "`nFuncionario B tentando atualizar com RowVersion antiga..."

try {
    Invoke-RestMethod `
        -Uri "$baseUrl/api/Usuarios/$idUsuario" `
        -Method PUT `
        -ContentType "application/json" `
        -Body $bodyB

    Write-Host "ERRO: Funcionario B conseguiu atualizar, mas deveria receber conflito."
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__

    if ($statusCode -eq 409) {
        Write-Host "SUCESSO: Funcionario B recebeu 409 Conflict."
        Write-Host "A concorrencia otimista de usuarios esta funcionando."
    }
    else {
        Write-Host "ERRO: Funcionario B falhou, mas com outro status:" $statusCode
        Write-Host $_.Exception.Message
    }
}