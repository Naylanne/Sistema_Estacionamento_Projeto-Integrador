// --- CONFIGURAÇÃO ---
// Verifique se a porta está correta (5196, 5000, 7201)
const API_URL = "http://localhost:5196/api";

document.addEventListener('DOMContentLoaded', () => {
    
    // --- 1. NAVEGAÇÃO ENTRE TELAS (Login -> Cadastro -> Recuperar) ---
    const loginForm = document.getElementById('loginForm');
    const cadastroForm = document.getElementById('cadastroForm');
    const recuperarSenhaForm = document.getElementById('recuperarSenhaForm');

    const btnCadastro = document.getElementById('btnCadastro');
    const btnRecuperarSenha = document.getElementById('btnRecuperarSenha');
    const btnVoltarLogin = document.getElementById('btnVoltarLogin');
    const btnVoltarLogin2 = document.getElementById('btnVoltarLogin2');

    // Mostrar tela de Cadastro
    btnCadastro.addEventListener('click', (e) => {
        e.preventDefault();
        loginForm.classList.add('hidden');
        cadastroForm.classList.remove('hidden');
    });

    // Mostrar tela de Recuperar Senha
    btnRecuperarSenha.addEventListener('click', (e) => {
        e.preventDefault();
        loginForm.classList.add('hidden');
        recuperarSenhaForm.classList.remove('hidden');
    });

    // Voltar para Login (Botão 1)
    btnVoltarLogin.addEventListener('click', (e) => {
        e.preventDefault();
        cadastroForm.classList.add('hidden');
        loginForm.classList.remove('hidden');
    });

    // Voltar para Login (Botão 2)
    btnVoltarLogin2.addEventListener('click', (e) => {
        e.preventDefault();
        recuperarSenhaForm.classList.add('hidden');
        loginForm.classList.remove('hidden');
    });


    // --- 2. LÓGICA DE LOGIN ---
    loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const cpf = document.getElementById('cpf').value;
        const senha = document.getElementById('senha').value;
        const tipoSelecionado = document.getElementById('tipoUsuario').value; // cliente ou admin

        if (!tipoSelecionado) {
            alert("Selecione o tipo de usuário!");
            return;
        }

        const btnEntrar = loginForm.querySelector('button[type="submit"]');
        const textoOriginal = btnEntrar.innerText;
        btnEntrar.innerText = "Verificando...";
        btnEntrar.disabled = true;

        try {
            // Busca todos os usuários do banco
            const response = await fetch(`${API_URL}/Usuarios`);
            if (!response.ok) throw new Error("Erro ao conectar com servidor");
            
            const usuarios = await response.json();

            // Procura usuário que bate CPF e Senha
            const usuarioEncontrado = usuarios.find(u => u.cpf === cpf && u.senhaAcesso === senha);

            if (usuarioEncontrado) {
                // Login Sucesso!
                // Salva dados no navegador para usar nas outras páginas
                localStorage.setItem('usuarioLogado', JSON.stringify(usuarioEncontrado));
                
                // Verifica se o tipo selecionado bate com a permissão (Simulação básica)
                // Na vida real, o tipo viria do banco, mas vamos confiar na seleção do HTML por enquanto
                // ou verificar se usuarioEncontrado.tipoUsuario bate.
                
                if (tipoSelecionado === 'admin') {
                    window.location.href = 'dashboard-admin.html';
                } else {
                    window.location.href = 'dashboard-cliente.html';
                }
            } else {
                alert("CPF ou Senha incorretos!");
            }

        } catch (erro) {
            console.error(erro);
            alert("Erro ao tentar fazer login. O servidor está rodando?");
        } finally {
            btnEntrar.innerText = textoOriginal;
            btnEntrar.disabled = false;
        }
    });


    // --- 3. LÓGICA DE CADASTRO ---
    cadastroForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const novoUsuario = {
            nome: document.getElementById('cadNome').value,
            cpf: document.getElementById('cadCpf').value,
            // dataNascimento: document.getElementById('cadDataNasc').value, // Backend precisa suportar DateOnly ou string
            telefone: document.getElementById('cadTelefone').value,
            endereco: document.getElementById('cadEndereco').value,
            senhaAcesso: document.getElementById('cadSenha').value,
            tipoUsuario: document.getElementById('cadTipoUsuario').value === 'admin' ? 'Funcionario' : 'Cliente',
            placaCarro: "SEM-PLACA", // Valor default
            modeloCarro: ""
        };

        try {
            const response = await fetch(`${API_URL}/Usuarios`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(novoUsuario)
            });

            if (response.ok) {
                alert("Cadastro realizado com sucesso! Faça login agora.");
                // Reseta form e volta pra tela de login
                cadastroForm.reset();
                cadastroForm.classList.add('hidden');
                loginForm.classList.remove('hidden');
            } else {
                const msgErro = await response.text();
                alert("Erro ao cadastrar: " + msgErro);
            }
        } catch (erro) {
            alert("Erro de conexão ao cadastrar.");
        }
    });

    // --- 4. RECUPERAR SENHA (Simulação) ---
    recuperarSenhaForm.addEventListener('submit', (e) => {
        e.preventDefault();
        const email = document.getElementById('recuperarEmail').value;
        
        // Simula tempo de processamento
        const btn = recuperarSenhaForm.querySelector('button[type="submit"]');
        btn.innerText = "Enviando...";
        
        setTimeout(() => {
            alert(`Um link de recuperação foi enviado para ${email}! Verifique sua caixa de entrada.`);
            recuperarSenhaForm.reset();
            recuperarSenhaForm.classList.add('hidden');
            loginForm.classList.remove('hidden');
            btn.innerText = "Recuperar Senha";
        }, 1500);
    });

});