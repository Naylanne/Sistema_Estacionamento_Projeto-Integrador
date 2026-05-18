const API_URL = "http://localhost:5196/api";

document.addEventListener('DOMContentLoaded', () => {
    // Navegação
    const links = document.querySelectorAll('.nav-link');
    const sections = document.querySelectorAll('.content-section');

    links.forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            links.forEach(l => l.classList.remove('active'));
            sections.forEach(s => s.classList.remove('active'));
            link.classList.add('active');
            document.getElementById(link.getAttribute('data-target')).classList.add('active');
            carregarDadosDaSecao(link.getAttribute('data-target'));
        });
    });

    carregarDashboard();
});

function carregarDadosDaSecao(secao) {
    switch(secao) {
        case 'dashboard': carregarDashboard(); break;
        case 'usuarios': carregarUsuarios(); break;
        case 'veiculos': carregarVeiculos(); break;
        case 'vagas': carregarVagas(); break;
        case 'tarifas': carregarTarifas(); break;
        case 'entrada-saida': carregarTabelasMovimentacao('estacionados'); break;
        case 'pagamentos': carregarTabelasMovimentacao('pagamentos'); break;
        case 'historico': carregarTabelasMovimentacao('todos'); break;
        case 'ocorrencias': carregarOcorrencias(); break;
        case 'avisos': carregarAvisos(); break;
        case 'feedbacks': carregarFeedbacks(); break;
    }
}

// --- MODAIS ---
function abrirModal(modalId) { document.getElementById(modalId).style.display = 'block'; }
function fecharModal(modalId) { document.getElementById(modalId).style.display = 'none'; }
window.onclick = function(event) { if (event.target.classList.contains('modal')) event.target.style.display = "none"; }

// --- DASHBOARD ---
async function carregarDashboard() {
    try {
        // 1. Carrega Vagas para contar ocupação
        const resVagas = await fetch(`${API_URL}/Vagas`);
        const vagas = await resVagas.json();
        
        const ocupadas = vagas.filter(v => v.status === 'Ocupada').length;
        const counters = document.querySelectorAll('.stat-number');
        
        if(counters.length >= 4) {
            counters[0].innerText = ocupadas; // Ocupadas
            counters[1].innerText = vagas.length - ocupadas; // Livres
        }

        // 2. Carrega Movimentação para calcular Faturamento do Dia
        const resMov = await fetch(`${API_URL}/Movimentacao`);
        const movimentos = await resMov.json();

        // Filtra pagamentos feitos HOJE
        const hoje = new Date().toLocaleDateString();
        const pagamentosHoje = movimentos.filter(m => 
            m.statusPagamento === 'Concluido' && 
            new Date(m.horaSaida).toLocaleDateString() === hoje
        );

        // Soma os valores
        const totalFaturamento = pagamentosHoje.reduce((acc, cur) => acc + cur.valorPago, 0);
        
        // Atualiza Card de Faturamento
        if(counters.length >= 3) {
            counters[2].innerText = `R$ ${totalFaturamento.toFixed(2)}`;
            document.querySelectorAll('.stat-total')[2].innerText = `${pagamentosHoje.length} veículos hoje`;
        }

        // Atualiza Card de Usuários Ativos
        const resUsers = await fetch(`${API_URL}/Usuarios`);
        const users = await resUsers.json();
        if(counters.length >= 4) {
            counters[3].innerText = users.length;
        }

        // 3. Renderiza Grid de Vagas
        const grid = document.getElementById('vagasGrid');
        grid.innerHTML = '';
        vagas.forEach(v => {
            const div = document.createElement('div');
            const statusClass = v.status === 'Disponivel' ? 'vaga-livre' : 'vaga-ocupada';
            // Estilo inline para garantir o visual do PDF
            div.className = `vaga-card ${statusClass}`;
            div.style.cssText = "padding:15px; margin:5px; border:2px solid #444; border-radius:8px; text-align:center; display:inline-block; width:100px; font-weight:bold;";
            
            // Cor condicional (Verde/Vermelho)
            const corTexto = v.status === 'Disponivel' ? '#03dac6' : '#cf6679';
            div.style.borderColor = corTexto;
            div.style.color = corTexto;

            div.innerHTML = `<div style="font-size:1.5em">${v.idVaga}</div><small style="color:#bbb">${v.tipoVaga}</small><br><span>${v.status}</span>`;
            grid.appendChild(div);
        });

    } catch(e) { console.error("Erro dashboard", e); }
}

// --- MOVIMENTAÇÃO ---
async function carregarTabelasMovimentacao(filtro) {
    let tbodyId = filtro === 'estacionados' ? 'tabelaEstacionados' : (filtro === 'pagamentos' ? 'tabelaPagamentos' : 'tabelaHistorico');
    const tbody = document.getElementById(tbodyId);
    if(!tbody) return;
    
    tbody.innerHTML = '<tr><td colspan="6">Carregando...</td></tr>';

    try {
        const res = await fetch(`${API_URL}/Movimentacao`);
        const dados = await res.json();
        tbody.innerHTML = '';

        const dadosFiltrados = dados.filter(item => {
            if(filtro === 'estacionados') return item.statusPagamento === 'Pendente';
            if(filtro === 'pagamentos') return item.statusPagamento === 'Concluido';
            return true;
        });

        dadosFiltrados.forEach(item => {
            const tr = document.createElement('tr');
            const entrada = new Date(item.horaEntrada).toLocaleString();
            const horas = Math.floor(item.tempoDecorrido / 60);
            const minutos = Math.floor(item.tempoDecorrido % 60);
            const tempoFormatado = `${horas}h ${minutos}min`;

            if(filtro === 'estacionados') {
                tr.innerHTML = `<td>${item.placa}</td><td>${item.idVaga}</td><td>${entrada}</td><td>${tempoFormatado}</td>
                                <td><button class="btn-small btn-warning" onclick="copiarTicket(${item.idAcesso})">Checkout</button></td>`;
            } else {
                tr.innerHTML = `<td>${entrada}</td><td>${item.placa}</td><td>${item.tipoVaga || 'Carro'}</td>
                                <td>${item.idVaga}</td><td>${tempoFormatado}</td><td>R$ ${item.valorPago.toFixed(2)}</td>`;
            }
            tbody.appendChild(tr);
        });
    } catch(e) { tbody.innerHTML = '<tr><td colspan="6">Erro ao carregar dados.</td></tr>'; }
}

function copiarTicket(id) {
    const campoSaida = document.getElementById('ticketSaida');
    if(campoSaida) {
        campoSaida.value = id;
        // Rola para a área de checkout
        document.querySelector('.es-action-card:nth-child(2)').scrollIntoView({behavior: "smooth"});
    }
}

// --- ENTRADA E SAÍDA ---
async function registrarEntrada() {
    const placaValor = document.getElementById('placaEntrada').value;
    const tipoValor = document.getElementById('tipoVeiculoEntrada').value;

    if(!placaValor) return alert("Digite a placa");

    const dados = { placa: placaValor, tipoVeiculo: tipoValor };

    try {
        const response = await fetch(`${API_URL}/Movimentacao/entrada`, {
            method: 'POST', headers: {'Content-Type': 'application/json'}, body: JSON.stringify(dados)
        });

        if(response.ok) {
            const ticket = await response.json();
            alert(`Entrada Registrada!\nVeículo: ${tipoValor}\nVaga: ${ticket.idVaga}`);
            document.getElementById('placaEntrada').value = '';
            carregarTabelasMovimentacao('estacionados');
            carregarDashboard();
        } else {
            alert("Erro: " + await response.text());
        }
    } catch(e) { alert("Erro de conexão."); }
}

async function registrarSaida() {
    const id = document.getElementById('ticketSaida').value;
    const selectPgto = document.getElementById('formaPagamentoSaida');
    const formaPgto = selectPgto ? selectPgto.value : "Dinheiro";

    if(!id) return alert("Digite o Ticket");

    try {
        const res = await fetch(`${API_URL}/Movimentacao/saida/${id}`, { 
            method: 'POST',
            headers: {'Content-Type': 'application/json'},
            body: JSON.stringify({ formaPagamento: formaPgto })
        });

        if(res.ok) { 
            const data = await res.json();
            alert(`Saída OK!\nValor: R$ ${data.valorFinal.toFixed(2)}\nPagamento: ${data.formaPagamento}`);
            document.getElementById('ticketSaida').value = '';
            document.getElementById('placaSaida').value = ''; // Limpa campo visual se houver
            carregarTabelasMovimentacao('estacionados');
            carregarDashboard();
        } else alert(await res.text());
    } catch(e) { alert("Erro na API"); }
}

// --- TARIFAS ---
async function carregarTarifas() {
    try {
        const res = await fetch(`${API_URL}/Tarifas`);
        const tarifas = await res.json();
        const t = tarifas.find(x => x.idTarifa === 1);
        if(t) {
            document.getElementById('comumPrimeiraHora').value = t.valorPrimeiraHora;
            document.getElementById('comumHoraAdicional').value = t.valorHoraAdicional;
            document.getElementById('comumDiaria').value = t.valorDiaria;
            document.getElementById('descontoFuncionario').value = t.descontoFuncionario;
            document.getElementById('descontoParceiro').value = t.descontoParceiro;
        }
    } catch(e) {}
}

async function salvarTarifas() {
    const tarifa = {
        idTarifa: 1, tipoTarifa: "Comum",
        valorPrimeiraHora: parseFloat(document.getElementById('comumPrimeiraHora').value),
        valorHoraAdicional: parseFloat(document.getElementById('comumHoraAdicional').value),
        valorDiaria: parseFloat(document.getElementById('comumDiaria').value),
        descontoFuncionario: parseFloat(document.getElementById('descontoFuncionario').value),
        descontoParceiro: parseFloat(document.getElementById('descontoParceiro').value)
    };
    try {
        const res = await fetch(`${API_URL}/Tarifas/1`, {
            method: 'PUT', headers: {'Content-Type': 'application/json'}, body: JSON.stringify(tarifa)
        });
        if(res.ok) alert("Tarifas salvas!");
        else alert("Erro ao salvar.");
    } catch(e) { alert("Erro de conexão."); }
}

// --- USUARIOS ---
async function carregarUsuarios() {
    const tbody = document.getElementById('tabelaUsuarios');
    try {
        const res = await fetch(`${API_URL}/Usuarios`);
        const lista = await res.json();
        tbody.innerHTML = '';
        lista.forEach(u => {
            const tr = document.createElement('tr');
            tr.innerHTML = `<td>${u.nome}</td><td>${u.cpf}</td><td>${u.tipoUsuario}</td><td>${u.telefone}</td><td>Ativo</td>
                            <td><button class="btn-small btn-danger" onclick="deletarUsuario(${u.idUsuario})">Excluir</button></td>`;
            tbody.appendChild(tr);
        });
    } catch(e) {}
}

async function deletarUsuario(id) {
    if(confirm("Excluir usuário?")) {
        await fetch(`${API_URL}/Usuarios/${id}`, { method: 'DELETE' });
        carregarUsuarios();
    }
}

async function salvarNovoUsuario() {
    const usuario = {
        nome: document.getElementById('modalUsuarioNome').value,
        cpf: document.getElementById('modalUsuarioCpf').value,
        telefone: document.getElementById('modalUsuarioTelefone').value,
        endereco: document.getElementById('modalUsuarioEndereco').value,
        tipoUsuario: document.getElementById('modalUsuarioTipo').value,
        senhaAcesso: document.getElementById('modalUsuarioSenha').value,
        placaCarro: "SEM-PLACA", modeloCarro: "" 
    };
    usuario.tipoUsuario = usuario.tipoUsuario.charAt(0).toUpperCase() + usuario.tipoUsuario.slice(1);

    await fetch(`${API_URL}/Usuarios`, {
        method: 'POST', headers: {'Content-Type': 'application/json'}, body: JSON.stringify(usuario)
    });
    fecharModal('modalNovoUsuario');
    carregarUsuarios();
}

// --- VAGAS ---
async function carregarVagas() {
    const tbody = document.getElementById('tabelaVagas');
    const res = await fetch(`${API_URL}/Vagas`);
    const lista = await res.json();
    tbody.innerHTML = '';
    lista.forEach(v => {
        const tr = document.createElement('tr');
        const btnDelete = v.status === 'Disponivel' 
                ? `<button class="btn-small btn-danger" onclick="deletarVaga(${v.idVaga})">Excluir</button>`
                : `<button class="btn-small" disabled style="opacity:0.5">Ocupada</button>`;
        tr.innerHTML = `<td>${v.idVaga}</td><td>${v.tipoVaga}</td><td>${v.status}</td><td>-</td><td>-</td><td>${btnDelete}</td>`;
        tbody.appendChild(tr);
    });
}

async function salvarNovaVaga() {
    const vaga = { tipoVaga: document.getElementById('modalVagaTipo').value, status: 'Disponivel' };
    const idManual = document.getElementById('modalVagaId').value;
    if(idManual) vaga.idVaga = parseInt(idManual);

    await fetch(`${API_URL}/Vagas`, {
        method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(vaga)
    });
    fecharModal('modalNovaVaga');
    carregarVagas();
    carregarDashboard();
}

async function deletarVaga(id) {
    if(confirm("Excluir vaga?")) {
        const res = await fetch(`${API_URL}/Vagas/${id}`, { method: 'DELETE' });
        if(res.ok) { carregarVagas(); carregarDashboard(); }
        else alert(await res.text());
    }
}

// --- VEÍCULOS ---
async function carregarVeiculos() {
    const tbody = document.getElementById('tabelaVeiculos');
    const res = await fetch(`${API_URL}/Usuarios`);
    const lista = await res.json();
    tbody.innerHTML = '';
    lista.filter(u => u.placaCarro && u.placaCarro !== "SEM-PLACA").forEach(u => {
        tr = document.createElement('tr');
        tr.innerHTML = `<td>${u.placaCarro}</td><td>${u.modeloCarro}</td><td>Carro</td><td>${u.nome}</td><td>Ativo</td>
                        <td><button class="btn-small btn-danger">Gerenciar</button></td>`;
        tbody.appendChild(tr);
    });
}

// --- AVISOS E OCORRÊNCIAS E FEEDBACKS ---
async function carregarAvisos() {
    const div = document.getElementById('listaAvisos');
    div.innerHTML = '';
    const res = await fetch(`${API_URL}/Avisos`);
    const lista = await res.json();
    lista.forEach(a => {
        const d = document.createElement('div');
        d.style = "background:#333; padding:10px; margin-bottom:5px";
        d.innerHTML = `<b>${a.titulo}</b><br>${a.descricao} <button style="float:right" class="btn-small btn-danger" onclick="deletarAviso(${a.idAviso})">X</button>`;
        div.appendChild(d);
    });
}
async function salvarNovoAviso() {
    await fetch(`${API_URL}/Avisos`, {
        method:'POST', headers:{'Content-Type':'application/json'},
        body: JSON.stringify({titulo: document.getElementById('modalAvisoTitulo').value, descricao: document.getElementById('modalAvisoDescricao').value})
    });
    fecharModal('modalNovoAviso'); carregarAvisos();
}
async function deletarAviso(id) { await fetch(`${API_URL}/Avisos/${id}`, {method:'DELETE'}); carregarAvisos(); }

async function carregarOcorrencias() {
    const tbody = document.getElementById('tabelaOcorrencias');
    const res = await fetch(`${API_URL}/Ocorrencias`);
    const lista = await res.json();
    tbody.innerHTML = '';
    lista.forEach(o => {
        const tr = document.createElement('tr');
        tr.innerHTML = `<td>${new Date(o.dataHora).toLocaleString()}</td><td>Ticket #${o.idAcesso}</td><td>${o.descricao}</td><td>Aberto</td><td>-</td>`;
        tbody.appendChild(tr);
    });
}
async function salvarNovaOcorrencia() {
    await fetch(`${API_URL}/Ocorrencias`, {
        method:'POST', headers:{'Content-Type':'application/json'},
        body: JSON.stringify({idAcesso: document.getElementById('modalOcorrenciaVeiculo').value, descricao: document.getElementById('modalOcorrenciaDescricao').value})
    });
    fecharModal('modalNovaOcorrencia'); carregarOcorrencias();
}

async function carregarFeedbacks() {
    const div = document.getElementById('listaFeedbacks');
    const res = await fetch(`${API_URL}/Feedbacks`);
    const lista = await res.json();
    div.innerHTML = '';
    lista.forEach(f => {
        div.innerHTML += `<div style="border-bottom:1px solid #444; padding:10px"><b>${f.usuario?.nome || 'Anônimo'}</b>: ${f.mensagem}</div>`;
    });
}