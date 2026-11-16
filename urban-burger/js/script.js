let categoriaAtual = 'todos';
let total = 0;
let unidades = 0;
let encontrados = 0;
let itensDoPedido = [];
let arquivoUrl = '';
let ultimoConteudo = '';
let codigoPedido = '';
let sequencia = 0;

function moeda(centavos) {
  return 'R$ ' + (centavos / 100).toFixed(2).replace('.', ',');
}
function aplicarTema() {
  let tema = localStorage.getItem('urban-burger-tema');
  let botao = document.querySelector('.theme-toggle');
  if (tema === 'dark') {
    document.documentElement.dataset.theme = 'dark';
    botao.textContent = 'Modo claro';
    botao.setAttribute('aria-pressed', 'true');
    botao.setAttribute('aria-label', 'Ativar modo claro');
  } else {
    document.documentElement.dataset.theme = 'light';
    botao.textContent = 'Modo escuro';
    botao.setAttribute('aria-pressed', 'false');
    botao.setAttribute('aria-label', 'Ativar modo escuro');
  }
}
function trocarTema() {
  if (document.documentElement.dataset.theme === 'dark') {
    localStorage.setItem('urban-burger-tema', 'light');
  } else {
    localStorage.setItem('urban-burger-tema', 'dark');
  }
  aplicarTema();
}
function fecharMenu() {
  document.getElementById('navegacao').classList.remove('open');
  document.querySelector('.mobile-toggle').setAttribute('aria-expanded', 'false');
  document.querySelector('.mobile-toggle').setAttribute('aria-label', 'Abrir menu');
}
function alternarMenu() {
  if (document.getElementById('navegacao').classList.contains('open')) {
    fecharMenu();
  } else {
    document.getElementById('navegacao').classList.add('open');
    document.querySelector('.mobile-toggle').setAttribute('aria-expanded', 'true');
    document.querySelector('.mobile-toggle').setAttribute('aria-label', 'Fechar menu');
  }
}
document.onkeydown = function(evento) {
  if (evento.key === 'Escape') {
    fecharMenu();
    fecharMensagem();
  }
};
document.onclick = function(evento) {
  if (!document.getElementById('navegacao').contains(evento.target) &&
      !document.querySelector('.mobile-toggle').contains(evento.target)) fecharMenu();
};
function mostrarMensagem(texto) {
  document.getElementById('mensagem').textContent = texto;
  document.querySelector('.toast').classList.add('visible');
}
function fecharMensagem() {
  document.querySelector('.toast').classList.remove('visible');
}
function textoPesquisa(texto) {
  return texto.toLowerCase().replace('á', 'a').replace('ã', 'a')
    .replace('â', 'a').replace('é', 'e').replace('ê', 'e')
    .replace('í', 'i').replace('ó', 'o').replace('õ', 'o')
    .replace('ô', 'o').replace('ú', 'u').replace('ç', 'c').trim();
}
function filtrarItem(id) {
  let produto = document.getElementById('produto-' + id);
  let busca = textoPesquisa(document.getElementById('pesquisa').value);
  let nome = textoPesquisa(produto.dataset.nome);
  produto.hidden = true;
  if (nome.indexOf(busca) >= 0 &&
      (categoriaAtual === 'todos' || categoriaAtual === produto.dataset.categoria)) {
    produto.hidden = false;
    encontrados = encontrados + 1;
  }
}
function filtrarProdutos() {
  if (!document.getElementById('pesquisa')) return;
  encontrados = 0;
  filtrarItem(1); filtrarItem(2); filtrarItem(3); filtrarItem(4); filtrarItem(5);
  filtrarItem(6); filtrarItem(7); filtrarItem(8); filtrarItem(9); filtrarItem(10);
  document.querySelector('.results-count').textContent = encontrados + ' produto(s) encontrado(s)';
  document.getElementById('sem-resultados').hidden = encontrados > 0;
}
function escolherCategoria(botao) {
  let anterior = document.querySelector('.category.active');
  anterior.classList.remove('active');
  anterior.setAttribute('aria-pressed', 'false');
  botao.classList.add('active');
  botao.setAttribute('aria-pressed', 'true');
  categoriaAtual = botao.dataset.category;
  filtrarProdutos();
}
function limparFiltros() {
  document.getElementById('pesquisa').value = '';
  escolherCategoria(document.querySelector('[data-category="todos"]'));
}
function lerQuantidade(id) {
  let quantidade = Number(localStorage.getItem('urban-quantidade-' + id));
  if (!(quantidade >= 0 && quantidade <= 99) || quantidade % 1 !== 0) return 0;
  return quantidade;
}
function alterarQuantidade(id, diferenca) {
  let quantidade = lerQuantidade(id) + diferenca;
  if (quantidade < 1 || quantidade > 99) {
    mostrarMensagem('Use de 1 a 99 unidades. Para excluir, clique em Remover.');
    return;
  }
  localStorage.setItem('urban-quantidade-' + id, quantidade);
  atualizarCarrinho();
  if (!document.getElementById('cart-content')) mostrarMensagem('Produto adicionado ao carrinho.');
}
function removerProduto(id) {
  localStorage.removeItem('urban-quantidade-' + id);
  atualizarCarrinho();
  mostrarMensagem('Produto removido do carrinho.');
}
function atualizarItem(id, preco) {
  let quantidade = lerQuantidade(id);
  unidades = unidades + quantidade;
  total = total + quantidade * preco;
  if (quantidade > 0) {
    itensDoPedido[itensDoPedido.length] = {
      produtoId: id, quantidade: quantidade, precoUnitario: preco / 100
    };
  }
  let item = document.getElementById('item-' + id);
  if (item) {
    item.hidden = quantidade === 0;
    document.getElementById('quantidade-' + id).textContent = quantidade;
    document.getElementById('subtotal-' + id).textContent = moeda(quantidade * preco);
    document.getElementById('menos-' + id).disabled = quantidade <= 1;
    document.getElementById('mais-' + id).disabled = quantidade >= 99;
  }
}
function atualizarCarrinho() {
  itensDoPedido = [];
  total = 0;
  unidades = 0;
  atualizarItem(1, 2890); atualizarItem(2, 3390); atualizarItem(3, 3890);
  atualizarItem(4, 3090); atualizarItem(5, 1490); atualizarItem(6, 1890);
  atualizarItem(7, 690); atualizarItem(8, 990); atualizarItem(9, 3990); atualizarItem(10, 7490);
  document.querySelector('.cart-count').textContent = unidades;
  document.querySelector('.cart-link').setAttribute('aria-label', 'Carrinho com ' + unidades + ' itens');
  if (document.getElementById('cart-content')) {
    invalidarArquivo();
    document.getElementById('carrinho-vazio').hidden = unidades > 0;
    document.getElementById('carrinho-preenchido').hidden = unidades === 0;
    document.getElementById('subtotal-pedido').textContent = moeda(total);
    document.getElementById('total-pedido').textContent = moeda(total);
    document.getElementById('checkout-feedback').hidden = true;
  }
}
function invalidarArquivo() {
  document.getElementById('baixar-pedido').hidden = true;
  document.getElementById('checkout-feedback').hidden = true;
  document.getElementById('pedido-nome').setCustomValidity('');
  document.getElementById('pedido-endereco').setCustomValidity('');
}
function finalizarPedido() {
  atualizarCarrinho();
  if (unidades === 0) return;
  let formulario = document.getElementById('dados-pedido');
  let nome = document.getElementById('pedido-nome');
  let endereco = document.getElementById('pedido-endereco');
  nome.value = nome.value.trim();
  endereco.value = endereco.value.trim();
  nome.setCustomValidity('');
  endereco.setCustomValidity('');
  if (nome.value.length < 3) nome.setCustomValidity('Informe um nome com pelo menos 3 caracteres.');
  if (endereco.value.length < 8) endereco.setCustomValidity('Informe um endereço com pelo menos 8 caracteres.');
  if (!validarCpf(document.getElementById('pedido-cpf'))) return;
  if (!formulario.reportValidity()) return;
  let pedido = {
    codigo: '',
    cliente: {
      nome: document.getElementById('pedido-nome').value.trim(),
      email: document.getElementById('pedido-email').value.trim(),
      cpf: document.getElementById('pedido-cpf').value,
      telefone: document.getElementById('pedido-telefone').value,
      endereco: document.getElementById('pedido-endereco').value.trim()
    },
    enderecoEntrega: document.getElementById('pedido-endereco').value.trim(),
    itens: itensDoPedido,
    valorTotal: total / 100
  };
  let conteudo = JSON.stringify(pedido);
  if (conteudo !== ultimoConteudo) {
    sequencia = sequencia + 1;
    codigoPedido = 'UB-' + Date.now() + '-' + sequencia;
    ultimoConteudo = conteudo;
  }
  pedido.codigo = codigoPedido;
  if (arquivoUrl !== '') URL.revokeObjectURL(arquivoUrl);
  let arquivo = new Blob([JSON.stringify(pedido, null, 2)], { type: 'application/json' });
  arquivoUrl = URL.createObjectURL(arquivo);
  let link = document.getElementById('baixar-pedido');
  link.href = arquivoUrl;
  link.download = codigoPedido + '.json';
  link.hidden = false;
  link.click();
  let aviso = document.getElementById('checkout-feedback');
  aviso.textContent = 'Pedido preparado. ';
  aviso.hidden = false;
  aviso.focus();
}
window.onpagehide = function() {
  if (arquivoUrl !== '') URL.revokeObjectURL(arquivoUrl);
};
function digitoValido(digito) {
  return digito === '' || (digito >= '0' && digito <= '9');
}
function validarCpf(campo) {
  let digitos = campo.value.replace('.', '').replace('.', '').replace('-', '');
  campo.setCustomValidity('');
  if (digitos.length !== 11 ||
      !(digitoValido(digitos.charAt(0)) && digitoValido(digitos.charAt(1)) &&
        digitoValido(digitos.charAt(2)) && digitoValido(digitos.charAt(3)) &&
        digitoValido(digitos.charAt(4)) && digitoValido(digitos.charAt(5)) &&
        digitoValido(digitos.charAt(6)) && digitoValido(digitos.charAt(7)) &&
        digitoValido(digitos.charAt(8)) && digitoValido(digitos.charAt(9)) &&
        digitoValido(digitos.charAt(10)))) {
    campo.setCustomValidity('Preencha os 11 dígitos do CPF.');
    campo.reportValidity();
    return false;
  }
  return true;
}
function mascara(campo) {
  if (campo.dataset.mask === 'cpf') campo.setCustomValidity('');
  let digitos = campo.value.replace('.', '').replace('.', '').replace('-', '')
    .replace('(', '').replace(')', '').replace(' ', '');
  digitos = digitos.slice(0, 11);
  if (!(digitoValido(digitos.charAt(0)) && digitoValido(digitos.charAt(1)) &&
        digitoValido(digitos.charAt(2)) && digitoValido(digitos.charAt(3)) &&
        digitoValido(digitos.charAt(4)) && digitoValido(digitos.charAt(5)) &&
        digitoValido(digitos.charAt(6)) && digitoValido(digitos.charAt(7)) &&
        digitoValido(digitos.charAt(8)) && digitoValido(digitos.charAt(9)) && digitoValido(digitos.charAt(10)))) {
    campo.value = campo.dataset.anterior || '';
    return;
  }
  let valor = digitos;
  if (campo.dataset.mask === 'cpf') {
    if (digitos.length > 3) valor = digitos.slice(0, 3) + '.' + digitos.slice(3);
    if (digitos.length > 6) valor = valor.slice(0, 7) + '.' + digitos.slice(6);
    if (digitos.length > 9) valor = valor.slice(0, 11) + '-' + digitos.slice(9);
  } else {
    if (digitos.length > 2) valor = '(' + digitos.slice(0, 2) + ') ' + digitos.slice(2);
    if (digitos.length > 7) valor = valor.slice(0, 10) + '-' + digitos.slice(7);
  }
  campo.value = valor;
  campo.dataset.anterior = valor;
}
function limparValidacao(formulario) {
  let confirmar = document.getElementById('cadastro-confirmacao');
  if (confirmar) confirmar.setCustomValidity('');
  formulario.querySelector('.form-feedback').hidden = true;
}
function enviarFormulario(formulario) {
  let aviso = formulario.querySelector('.form-feedback');
  if (formulario.id === 'form-cadastro') {
    if (!validarCpf(document.getElementById('cadastro-cpf'))) return false;
    let senha = document.getElementById('cadastro-senha');
    let confirmar = document.getElementById('cadastro-confirmacao');
    if (senha.value !== confirmar.value) {
      confirmar.setCustomValidity('As senhas precisam ser iguais.');
      confirmar.reportValidity();
      return false;
    }
    senha.value = '';
    confirmar.value = '';
    aviso.textContent = 'Cadastro indisponível. Nenhuma conta foi criada; continue pelo carrinho sem cadastro.';
  } else if (formulario.id === 'form-login') {
    document.getElementById('login-senha').value = '';
    aviso.textContent = 'Acesso à conta indisponível. Continue pelo carrinho sem entrar.';
  }
  aviso.hidden = false;
  aviso.focus();
  return false;
}

aplicarTema();
atualizarCarrinho();
filtrarProdutos();
