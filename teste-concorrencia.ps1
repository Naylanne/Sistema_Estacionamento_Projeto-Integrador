$api = "http://localhost:5196"

function Criar-Veiculo {
    param(
        [string]$Placa,
        [string]$Modelo
    )

    $body = @{
        placa = $Placa
        modelo = $Modelo
        tipoVeiculo = "Carro"
        idUsuario = 1
    } | ConvertTo-Json

    try {
        Invoke-RestMethod -Uri "$api/api/Veiculos" -Method Post -ContentType "application/json" -Body $body | Out-Null
        Write-Host "Veiculo criado: $Placa"
    }
    catch {
        Write-Host "Veiculo ja existente ou nao criado: $Placa"
    }
}

# Veículo usado para ocupar uma das duas vagas de carro antes do teste.
Criar-Veiculo -Placa "BASE001" -Modelo "Veiculo Base"

# Veículos usados no teste concorrente.
$placas = @("CONC001", "CONC002", "CONC003", "CONC004", "CONC005")

foreach ($placa in $placas) {
    Criar-Veiculo -Placa $placa -Modelo "Veiculo Concorrencia"
}

Write-Host ""
Write-Host "Ocupando uma vaga de carro antes do teste concorrente..."

$bodyBase = @{
    placa = "BASE001"
    tipoVeiculo = "Carro"
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri "$api/api/Acessos/entrada" -Method Post -ContentType "application/json" -Body $bodyBase | Out-Null
    Write-Host "Vaga base ocupada com BASE001."
}
catch {
    Write-Host "Vaga base já estava ocupada ou entrada já registrada."
}

Write-Host ""
Write-Host "Iniciando teste concorrente com 5 requisições simultâneas..."
Write-Host ""

$jobs = foreach ($placa in $placas) {
    Start-Job -ArgumentList $api, $placa -ScriptBlock {
        param($api, $placa)

        $body = @{
            placa = $placa
            tipoVeiculo = "Carro"
        } | ConvertTo-Json

        try {
            $response = Invoke-WebRequest -Uri "$api/api/Acessos/entrada" -Method Post -ContentType "application/json" -Body $body -UseBasicParsing

            [PSCustomObject]@{
                Placa = $placa
                StatusCode = [int]$response.StatusCode
                Resultado = $response.Content
            }
        }
        catch {
            $statusCode = $_.Exception.Response.StatusCode.value__

            $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
            $responseBody = $reader.ReadToEnd()

            [PSCustomObject]@{
                Placa = $placa
                StatusCode = [int]$statusCode
                Resultado = $responseBody
            }
        }
    }
}

$resultados = $jobs | Wait-Job | Receive-Job
$jobs | Remove-Job

$resultados | Sort-Object StatusCode | Format-Table -AutoSize

$resultados | Export-Csv ".\resultado-concorrencia.csv" -NoTypeInformation -Encoding UTF8

Write-Host ""
Write-Host "Resultado salvo em resultado-concorrencia.csv"