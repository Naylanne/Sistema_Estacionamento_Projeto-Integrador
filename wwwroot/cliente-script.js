const API_URL = "http://localhost:5196/api";

let usuarioLogado = null;
let timerInterval = null;
let ticketAtivo = null;
let minhasPlacas = []; // Lista para armazenar todos os carros do cliente

document.addEventListener('DOMContentLoaded', () => {
    const dadosLocal = localStorage.getItem('usuarioLogado');
    if (!dadosLocal) { alert("Login necessário!"); window.location.href = "index.html"; return; }
    
    usuarioLogado = JSON.parse(dadosLocal);
    
    const spanNome = document.querySelector('.user-section span');
    if(spanNome) spanNome.innerText = `Bem-vindo, ${usuarioLogado.nome.split(' ')[0]}`;

    resetarCardsDashboard();

    // Navegação
    document.querySelectorAll('.nav-link').forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            document.querySelectorAll('.nav-link').forEach(l => l.classList.remove('active'));
            document.querySelectorAll('.content-section').forEach(s => s.classList.remove('active'));
            link.classList.add('active');
            document.getElementById(link.getAttribute('data-target')).classList.add('active');
            
            const target = link.getAttribute('data-target');
            if (target === 'meus-dados') preencherMeusDados();
            if (target === 'meus-veiculos') carregarVeiculos();
            if (target === 'historico' || target === 'pagamentos' || target === 'dashboard') carregarDadosCompletos();
            if (target === 'avisos') carregarAvisos();
        });
    });

    document.querySelector('.btn-logout').addEventListener('click', () => {
        localStorage.removeItem('usuarioLogado'); window.location.href = 'index.html';
    });

    // Inicialização sequencial correta
    preencherMeusDados();
    carregarAvisos();
    // Carrega veículos e DEPOIS carrega o histórico (pois histórico depende das placas)
    carregarVeiculos().then(() => carregarDadosCompletos());
});

// --- LÓGICA PRINCIPAL ---

async function carregarVeiculos() {
    const grid = document.querySelector('.veiculos-grid');
    if(grid) grid.innerHTML = '<p>Carregando...</p>';
    minhasPlacas = []; // Reseta lista

    try {
        // Busca veículos na API nova
        const res = await fetch(`${API_URL}/Veiculos/usuario/${usuarioLogado.idUsuario}`);
        const veiculos = await res.json();

        if(grid) grid.innerHTML = '';
        
        if (veiculos.length === 0) {
            if(grid) grid.innerHTML = '<p>Nenhum veículo cadastrado.</p>';
            return;
        }

        veiculos.forEach(v => {
            minhasPlacas.push(v.placa); // Guarda placa para usar no filtro do histórico
            if(grid) {
                const card = document.createElement('div');
                card.className = 'veiculo-card';
                card.innerHTML = `
                    <div class="veiculo-header"><span class="placa">${v.placa}</span><span class="status livre">Cadastrado</span></div>
                    <div class="veiculo-info"><span class="modelo">${v.modelo}</span><span class="tipo">${v.tipoVeiculo}</span></div>
                    <div class="veiculo-actions"><button class="btn-small btn-danger" onclick="deletarVeiculo(${v.idVeiculo})">Remover</button></div>`;
                grid.appendChild(card);
            }
        });
    } catch(e) { console.error(e); }
}

async function carregarDadosCompletos() {
    if(minhasPlacas.length === 0) {
        resetarCardsDashboard();
        return;
    }

    try {
        const res = await fetch(`${API_URL}/Movimentacao`);
        const todos = await res.json();
        
        // Filtra onde a placa do acesso está na minha lista de placas
        const meusAcessos = todos.filter(a => minhasPlacas.includes(a.placa));
        
        preencherTabela('tabelaHistorico', meusAcessos);
        preencherTabela('tabelaDashboard', meusAcessos.slice(0, 3)); // Mostra 3 últimos
        preencherTabela('tabelaPagamentos', meusAcessos.filter(a => a.statusPagamento === 'Concluido'));

        // Procura ticket ativo em qualquer um dos carros
        ticketAtivo = meusAcessos.find(a => a.statusPagamento === 'Pendente');
        
        if(ticketAtivo) {
            iniciarTimerDashboard(ticketAtivo.horaEntrada);
            atualizarCardStatus(true, ticketAtivo);
        } else {
            pararTimerDashboard();
            resetarCardsDashboard();
        }
    } catch(e) { console.error(e); }
}

// --- UI HELPERS ---

function preencherTabela(id, dados) {
    const tbody = document.getElementById(id);
    if(!tbody) return;
    tbody.innerHTML = '';
    if(dados.length === 0) { tbody.innerHTML = '<tr><td colspan="6">Nada encontrado.</td></tr>'; return; }
    
    dados.forEach(i => {
        const entrada = new Date(i.horaEntrada).toLocaleString();
        const saida = i.horaSaida ? new Date(i.horaSaida).toLocaleString() : '-';
        const valor = i.valorPago > 0 ? `R$ ${i.valorPago.toFixed(2)}` : '-';
        tbody.innerHTML += `<tr><td>${entrada}</td><td>${saida}</td><td>${i.placa}</td><td>${i.idVaga}</td><td>${i.valorPago ? 'Pago' : 'Pendente'}</td><td>${valor}</td></tr>`;
    });
}

function iniciarTimerDashboard(entradaISO) {
    if(timerInterval) clearInterval(timerInterval);
    const dataEntrada = new Date(entradaISO);
    const atualizar = () => {
        const diff = new Date() - dataEntrada;
        if(diff < 0) return;
        const hrs = Math.floor(diff / 3600000);
        const mins = Math.floor((diff % 3600000) / 60000);
        
        const cards = document.querySelectorAll('.stat-number');
        if(cards.length >= 3) {
            cards[1].innerText = `${hrs}h ${mins}min`;
            let est = 10.00 + (hrs >= 1 ? hrs * 5.00 : 0); // Regra visual simples
            cards[2].innerText = `R$ ${est.toFixed(2)}*`;
        }
    };
    atualizar();
    timerInterval = setInterval(atualizar, 60000);
}

function pararTimerDashboard() { if(timerInterval) clearInterval(timerInterval); }

function atualizarCardStatus(ativo, ticket) {
    const cards = document.querySelectorAll('.stat-number');
    const totais = document.querySelectorAll('.stat-total');
    if(ativo) {
        cards[0].innerText = "Estacionado";
        cards[0].style.color = "#03dac6";
        totais[0].innerText = `${ticket.placa} na Vaga ${ticket.idVaga}`;
        
        const badge = document.querySelector('.placa-badge');
        if(badge) badge.innerText = ticket.placa;
        document.querySelector('.veiculo-modelo').innerText = "Veículo Ativo";
        document.querySelector('.vaga-info').innerHTML = `Vaga: ${ticket.idVaga}`;
        
        const btn = document.querySelector('.veiculo-info button');
        if(btn) { btn.disabled = false; btn.innerText = "Realizar Pagamento"; btn.onclick = () => abrirModal('modalPagamento'); }
    }
}

function resetarCardsDashboard() {
    const cards = document.querySelectorAll('.stat-number');
    if(cards.length >= 3) { cards[0].innerText = "Ausente"; cards[0].style.color="#888"; cards[1].innerText="--:--"; cards[2].innerText="R$ 0,00"; }
    const btn = document.querySelector('.veiculo-info button');
    if(btn) { btn.disabled = true; btn.innerText = "Nenhum veículo estacionado"; }
    const badge = document.querySelector('.placa-badge');
    if(badge) badge.innerText = "---";
}

// --- AÇÕES ---

async function processarPagamento() {
    const forma = document.getElementById('formaPagamento').value;
    if(!forma) return alert("Selecione forma de pagamento");
    
    await fetch(`${API_URL}/Movimentacao/saida/${ticketAtivo.idAcesso}`, {
        method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({formaPagamento:forma})
    });
    alert("Pagamento Confirmado!"); fecharModal('modalPagamento'); carregarDadosCompletos();
}

async function salvarNovoVeiculoCliente() {
    const v = {
        placa: document.getElementById('modalVeiculoPlaca').value,
        modelo: document.getElementById('modalVeiculoModelo').value,
        tipoVeiculo: document.getElementById('modalVeiculoTipo').value,
        idUsuario: usuarioLogado.idUsuario
    };
    await fetch(`${API_URL}/Veiculos`, { method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify(v) });
    fecharModal('modalNovoVeiculoCliente');
    carregarVeiculos().then(() => carregarDadosCompletos()); // Recarrega tudo pois placa nova pode ter histórico
}

async function deletarVeiculo(id) {
    if(confirm("Excluir?")) { await fetch(`${API_URL}/Veiculos/${id}`, {method:'DELETE'}); carregarVeiculos(); }
}

// --- GERAL (Dados, Avisos, Feedback) ---
function preencherMeusDados() {
    const inputs = document.querySelectorAll('#formEditarDados input');
    if(inputs.length>=3) { inputs[0].value=usuarioLogado.nome; inputs[1].value=usuarioLogado.telefone; inputs[2].value=usuarioLogado.endereco; }
    const spans = document.querySelectorAll('.dados-grid span');
    if(spans.length>=5) { spans[0].innerText=usuarioLogado.nome; spans[1].innerText=usuarioLogado.cpf; spans[3].innerText=usuarioLogado.telefone; spans[4].innerText=usuarioLogado.endereco; }
}
async function salvarDados() {
    const inputs = document.querySelectorAll('#formEditarDados input');
    usuarioLogado.nome=inputs[0].value; usuarioLogado.telefone=inputs[1].value; usuarioLogado.endereco=inputs[2].value;
    await fetch(`${API_URL}/Usuarios/${usuarioLogado.idUsuario}`, {method:'PUT', headers:{'Content-Type':'application/json'}, body:JSON.stringify(usuarioLogado)});
    localStorage.setItem('usuarioLogado', JSON.stringify(usuarioLogado));
    preencherMeusDados(); fecharModal('modalEditarDados');
}
async function carregarAvisos() {
    const div = document.querySelector('.avisos-list');
    if(!div) return;
    div.innerHTML='';
    const res = await fetch(`${API_URL}/Avisos`);
    (await res.json()).forEach(a => div.innerHTML+=`<div class="aviso-item"><h4>${a.titulo}</h4><p>${a.descricao}</p></div>`);
}
async function enviarFeedback() {
    const msg = document.getElementById('mensagemFeedback').value;
    await fetch(`${API_URL}/Feedbacks`, {method:'POST', headers:{'Content-Type':'application/json'}, body:JSON.stringify({idUsuario:usuarioLogado.idUsuario, mensagem:msg, dataEnvio:new Date().toISOString()})});
    alert("Enviado!"); fecharModal('modalNovoFeedback');
}

// Modais
function abrirModal(id) { 
    document.getElementById(id).style.display='block'; 
    if(id==='modalPagamento' && ticketAtivo) {
        document.getElementById('resumoPlaca').innerText = ticketAtivo.placa;
        const val = document.querySelectorAll('.stat-number')[2].innerText;
        document.getElementById('resumoValor').innerText = val;
        // Atualiza tempo no modal também
        const diff = new Date() - new Date(ticketAtivo.horaEntrada);
        const hrs = Math.floor(diff/3600000);
        const mins = Math.floor((diff%3600000)/60000);
        document.getElementById('resumoTempo').innerText = `${hrs}h ${mins}min`;
        
        // Toggle Pix/Cartao
        const sel = document.getElementById('formaPagamento');
        sel.onchange = (e) => {
            document.getElementById('detalhesPix').classList.add('hidden');
            document.getElementById('detalhesCartao').classList.add('hidden');
            if(e.target.value==='Pix') document.getElementById('detalhesPix').classList.remove('hidden');
            if(e.target.value==='Cartao') document.getElementById('detalhesCartao').classList.remove('hidden');
        }
    }
}
function fecharModal(id) { document.getElementById(id).style.display='none'; }
window.onclick = (e) => { if(e.target.classList.contains('modal')) e.target.style.display='none'; }