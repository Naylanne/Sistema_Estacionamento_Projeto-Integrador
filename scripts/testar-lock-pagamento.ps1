param(
    [string]$BaseUrl = "http://localhost:5196",
    [int]$IdAcesso = 1
)

Write-Host "=== TESTE DE LOCK PESSIMISTA - PAGAMENTO ===" -ForegroundColor Cyan
Write-Host "API: $BaseUrl"
Write-Host "IdAcesso testado: $IdAcesso"
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

$url = "$BaseUrl/api/Acessos/saida/$IdAcesso"

$bodyA = @{
    formaPagamento = "Pix"
}

$bodyB = @{
    formaPagamento = "CartaoDebito"
}

$inicio = (Get-Date).AddSeconds(2)
$jobs = @()

Write-Host "Cenario: duas requisicoes tentando registrar saida/pagamento para o mesmo acesso." -ForegroundColor Yellow
Write-Host "Disparando requisicoes concorrentes em 2 segundos..."
Write-Host ""

$jobs += Start-RequisicaoConcorrente `
    -Nome "Funcionario A - Pagamento" `
    -Metodo "POST" `
    -Url $url `
    -Body $bodyA `
    -Inicio $inicio

$jobs += Start-RequisicaoConcorrente `
    -Nome "Funcionario B - Pagamento" `
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

$sucessos = @($resultados | Where-Object {
    $_.Status -is [int] -and $_.Status -ge 200 -and $_.Status -lt 300
}).Count

$erros = @($resultados | Where-Object {
    -not ($_.Status -is [int] -and $_.Status -ge 200 -and $_.Status -lt 300)
}).Count

Write-Host ""
Write-Host "=== ANALISE ===" -ForegroundColor Cyan

if ($sucessos -eq 1 -and $erros -ge 1) {
    Write-Host "SUCESSO: o lock pessimista no pagamento/saida funcionou." -ForegroundColor Green
    Write-Host "Uma requisicao registrou a saida/pagamento e a outra foi impedida de duplicar a operacao."
}
elseif ($sucessos -gt 1) {
    Write-Host "ATENCAO: as duas requisicoes registraram pagamento/saida." -ForegroundColor Red
    Write-Host "Isso pode indicar duplicidade no fluxo de pagamento."
}
else {
    Write-Host "TESTE NAO CONCLUSIVO: nenhuma requisicao foi aceita." -ForegroundColor Yellow
    Write-Host "Verifique se o IdAcesso existe e se a saida ainda nao foi registrada."
}