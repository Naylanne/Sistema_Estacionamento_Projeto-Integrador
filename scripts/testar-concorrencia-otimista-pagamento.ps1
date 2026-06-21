param(
    [int]$IdPagamento = 2,
    [string]$BaseUrl = "http://localhost:5196",
    [ValidateSet("Pago", "Concluido")]
    [string]$StatusFuncionarioA = "Concluido",
    [ValidateSet("Pendente", "Pago", "Concluido", "Cancelado")]
    [string]$StatusFuncionarioB = "Cancelado"
)

[Console]::InputEncoding = [System.Text.Encoding]::UTF8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

function Set-JsonProp {
    param(
        [Parameter(Mandatory = $true)]
        $Obj,

        [Parameter(Mandatory = $true)]
        [string[]]$Names,

        [Parameter(Mandatory = $true)]
        $Value
    )

    $found = $false

    foreach ($name in $Names) {
        $prop = $Obj.PSObject.Properties | Where-Object { $_.Name -ieq $name } | Select-Object -First 1

        if ($null -ne $prop) {
            $prop.Value = $Value
            $found = $true
        }
    }

    if (-not $found) {
        $Obj | Add-Member -NotePropertyName $Names[0] -NotePropertyValue $Value -Force
    }
}

function Invoke-ApiPut {
    param(
        [string]$Url,
        $Body
    )

    $json = $Body | ConvertTo-Json -Depth 10

    try {
        $response = Invoke-WebRequest `
            -Uri $Url `
            -Method Put `
            -Body $json `
            -ContentType "application/json" `
            -UseBasicParsing

        return @{
            StatusCode = [int]$response.StatusCode
            Body = $response.Content
        }
    }
    catch {
        $statusCode = 0
        $responseBody = $_.Exception.Message

        if ($_.Exception.Response -ne $null) {
            $statusCode = [int]$_.Exception.Response.StatusCode

            try {
                $stream = $_.Exception.Response.GetResponseStream()
                $reader = New-Object System.IO.StreamReader($stream)
                $responseBody = $reader.ReadToEnd()
            }
            catch {
                $responseBody = $_.Exception.Message
            }
        }

        return @{
            StatusCode = $statusCode
            Body = $responseBody
        }
    }
}

Write-Host "=== TESTE DE CONCORRENCIA OTIMISTA - PAGAMENTO ==="
Write-Host ""

$urlPagamento = "$BaseUrl/api/Pagamento/$IdPagamento"

try {
    Write-Host "Funcionario A lendo pagamento..."
    $pagamentoA = Invoke-RestMethod -Uri $urlPagamento -Method Get

    Write-Host "Funcionario B lendo pagamento..."
    $pagamentoB = Invoke-RestMethod -Uri $urlPagamento -Method Get
}
catch {
    $statusCode = 0
    $responseBody = $_.Exception.Message

    if ($_.Exception.Response -ne $null) {
        $statusCode = [int]$_.Exception.Response.StatusCode
    }

    Write-Host "Erro ao buscar pagamento. Status: $statusCode"
    Write-Host $responseBody
    exit
}

# Clona os objetos para nao alterar diretamente os retornos originais
$bodyA = $pagamentoA | ConvertTo-Json -Depth 10 | ConvertFrom-Json
$bodyB = $pagamentoB | ConvertTo-Json -Depth 10 | ConvertFrom-Json

# Funcionario A tenta concluir/pagar o pagamento
Set-JsonProp -Obj $bodyA -Names @("statusPagamento", "StatusPagamento") -Value $StatusFuncionarioA

# Funcionario B tenta alterar o mesmo pagamento com RowVersion antiga
Set-JsonProp -Obj $bodyB -Names @("statusPagamento", "StatusPagamento") -Value $StatusFuncionarioB

Write-Host ""
Write-Host "Funcionario A atualizando pagamento para '$StatusFuncionarioA'..."
$resultadoA = Invoke-ApiPut -Url $urlPagamento -Body $bodyA
if ($resultadoA.StatusCode -eq 204) {
    Write-Host "Status Funcionario A: Alterado com sucesso!"
}
else {
    Write-Host "Status Funcionario A: $($resultadoA.StatusCode)"
}

Write-Host ""
Write-Host "Funcionario B tentando atualizar para '$StatusFuncionarioB' com RowVersion antiga..."
$resultadoB = Invoke-ApiPut -Url $urlPagamento -Body $bodyB
Write-Host "Status Funcionario B: $($resultadoB.StatusCode)"

Write-Host ""

if ($resultadoA.StatusCode -eq 204 -and $resultadoB.StatusCode -eq 409) {
    Write-Host "SUCESSO: Concorrencia otimista em Pagamento funcionando."
    Write-Host "Funcionario A atualizou para '$StatusFuncionarioA' e Funcionario B recebeu 409 Conflict ao tentar '$StatusFuncionarioB'."
}
else {
    Write-Host "RESULTADO INESPERADO."
    Write-Host ""
    Write-Host "Resposta A:"
    Write-Host $resultadoA.Body
    Write-Host ""
    Write-Host "Resposta B:"
    Write-Host $resultadoB.Body
}