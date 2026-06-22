param(
    [string]$BaseUrl = "http://localhost:5196",
    [string]$PlacaA = "ABC1234",
    [string]$PlacaB = "DEF5678",
    [string]$TipoVeiculo = "Carro"
)

Write-Host "=== TESTE DE LOCK PESSIMISTA - VAGA ===" -ForegroundColor Cyan
Write-Host "API: $BaseUrl"
Write-Host "Placa A: $PlacaA"
Write-Host "Placa B: $PlacaB"
Write-Host ""

function Start-RequisicaoConcorrente {
    param(
        [string]$Nome,
        [string]$Metodo,
        [string]$Url,
        [object]$Body,
        [datetime]$Inicio
    )

    $bodyJson = $Body | ConvertTo-Json -Depth 10

    Start-Job -ScriptBlock {
        param($Nome, $Metodo, $Url, $BodyJson, $Inicio)

        while ((Get-Date) -lt $Inicio) {
            Start-Sleep -Milliseconds 10
        }

        $sw = [System.Diagnostics.Stopwatch]::StartNew()

        try {
            $resposta = Invoke-WebRequest `
                -Uri $Url `
                -Method $Metodo `
                -Headers @{
                    "accept" = "*/*"
                    "Content-Type" = "application/json"
                } `
                -Body $BodyJson `
                -UseBasicParsing

            $sw.Stop()

            [PSCustomObject]@{
                Requisicao = $Nome
                Status     = [int]$resposta.StatusCode
                Resultado  = "SUCESSO"
                DuracaoMs  = $sw.ElapsedMilliseconds
                Corpo      = $resposta.Content
                Body       = $BodyJson
            }
        }
        catch {
            $sw.Stop()

            $status = "SEM_STATUS"
            $corpo = ""

            if ($_.Exception.Response) {
                try {
                    $status = [int]$_.Exception.Response.StatusCode
                }
                catch {
                    $status = "ERRO"
                }
            }

            if ($_.ErrorDetails.Message) {
                $corpo = $_.ErrorDetails.Message
            }
            elseif ($_.Exception.Response) {
                try {
                    $stream = $_.Exception.Response.GetResponseStream()
                    $reader = New-Object System.IO.StreamReader($stream)
                    $corpo = $reader.ReadToEnd()
                }
                catch {
                    $corpo = $_.Exception.Message
                }
            }
            else {
                $corpo = $_.Exception.Message
            }

            [PSCustomObject]@{
                Requisicao = $Nome
                Status     = $status
                Resultado  = "ERRO"
                DuracaoMs  = $sw.ElapsedMilliseconds
                Corpo      = $corpo
                Body       = $BodyJson
            }
        }

    } -ArgumentList $Nome, $Metodo, $Url, $bodyJson, $Inicio
}

Write-Host "Consultando vagas antes do teste..." -ForegroundColor Yellow

try {
    $vagas = Invoke-RestMethod "$BaseUrl/api/Vagas"
    $vagasCarroDisponiveis = @($vagas | Where-Object {
        $_.tipoVaga -eq "Carro" -and $_.status -eq "Disponivel"
    })

    Write-Host "Vagas de carro disponiveis: $($vagasCarroDisponiveis.Count)"

    if ($vagasCarroDisponiveis.Count -gt 1) {
        Write-Host "ATENCAO: existe mais de uma vaga de carro disponivel." -ForegroundColor Yellow
        Write-Host "Para provar disputa pela mesma vaga, deixe apenas uma vaga de carro disponivel."
        Write-Host ""
    }

    if ($vagasCarroDisponiveis.Count -eq 0) {
        Write-Host "ATENCAO: nenhuma vaga de carro disponivel." -ForegroundColor Yellow
        Write-Host "O teste provavelmente nao aceitara nenhuma entrada."
        Write-Host ""
    }
}
catch {
    Write-Host "Nao foi possivel consultar as vagas antes do teste." -ForegroundColor Yellow
}

$url = "$BaseUrl/api/Acessos/entrada"

$bodyA = @{
    placa       = $PlacaA
    tipoVeiculo = $TipoVeiculo
}

$bodyB = @{
    placa       = $PlacaB
    tipoVeiculo = $TipoVeiculo
}

$inicio = (Get-Date).AddSeconds(2)
$jobs = @()

Write-Host "Cenario: dois veiculos diferentes tentando ocupar vaga disponivel ao mesmo tempo." -ForegroundColor Yellow
Write-Host "Disparando requisicoes concorrentes em 2 segundos..."
Write-Host ""

$jobs += Start-RequisicaoConcorrente `
    -Nome "Funcionario A - Ocupacao de vaga" `
    -Metodo "POST" `
    -Url $url `
    -Body $bodyA `
    -Inicio $inicio

$jobs += Start-RequisicaoConcorrente `
    -Nome "Funcionario B - Ocupacao de vaga" `
    -Metodo "POST" `
    -Url $url `
    -Body $bodyB `
    -Inicio $inicio

$resultados = $jobs | Wait-Job | Receive-Job
$jobs | Remove-Job

Write-Host "=== RESULTADOS ===" -ForegroundColor Cyan
$resultados | Format-Table Requisicao, Status, Resultado, DuracaoMs -AutoSize

Write-Host ""
Write-Host "=== BODIES ENVIADOS ===" -ForegroundColor Cyan
foreach ($r in $resultados) {
    Write-Host ""
    Write-Host "[$($r.Requisicao)]"
    Write-Host $r.Body
}

Write-Host ""
Write-Host "=== CORPO DAS RESPOSTAS ===" -ForegroundColor Cyan
foreach ($r in $resultados) {
    Write-Host ""
    Write-Host "[$($r.Requisicao)] Status: $($r.Status)" -ForegroundColor Yellow
    Write-Host $r.Corpo
}

Write-Host ""
Write-Host "=== VAGAS APOS O TESTE ===" -ForegroundColor Cyan
try {
    Invoke-RestMethod "$BaseUrl/api/Vagas" | Format-Table idVaga, tipoVaga, status
}
catch {
    Write-Host "Nao foi possivel consultar as vagas apos o teste."
}

$sucessos = @($resultados | Where-Object {
    $_.Status -is [int] -and $_.Status -ge 200 -and $_.Status -lt 300
}).Count

$erros = @($resultados | Where-Object {
    -not ($_.Status -is [int] -and $_.Status -ge 200 -and $_.Status -lt 300)
}).Count

Write-Host ""
Write-Host "=== ANALISE ===" -ForegroundColor Cyan

if ($sucessos -eq 1 -and $erros -ge 1) {
    Write-Host "SUCESSO: o lock pessimista na ocupacao de vaga funcionou." -ForegroundColor Green
    Write-Host "Dois veiculos disputaram vaga disponivel, mas apenas uma ocupacao foi efetivada."
}
elseif ($sucessos -gt 1) {
    Write-Host "ATENCAO: as duas entradas foram aceitas." -ForegroundColor Yellow
    Write-Host "Isso pode ser normal se havia mais de uma vaga de carro disponivel."
    Write-Host "Para evidenciar lock de vaga, deixe apenas uma vaga de carro disponivel e rode novamente."
}
else {
    Write-Host "TESTE NAO CONCLUSIVO: nenhuma requisicao foi aceita." -ForegroundColor Yellow
    Write-Host "Verifique se os veiculos existem, se nao estao ativos e se ha vaga disponivel."
}