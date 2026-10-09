# Vander Limp — Planejamento do sistema

> Versão 1.0 — 09/10/2026  
> Situação: planejamento. A implementação ainda não foi iniciada.

## 1. Objetivo

Criar um sistema de venda de produtos de limpeza com aplicativo .NET MAUI para clientes, painel administrativo web, API e banco MySQL. O Vander Limp será o único fornecedor.

O cliente poderá cadastrar-se, consultar produtos, montar e repetir pedidos, pagar por PIX ou na entrega e acompanhar a compra. Os dados oficiais ficarão no servidor, com cópias e rascunhos no aparelho.

## 2. Escopo da primeira versão

- Cadastro de pessoa física e jurídica, sem aprovação manual.
- Autenticação e sessão persistente para acesso automático.
- Endereço cadastral e endereços de entrega distintos.
- Catálogo atualizado pela API, mostrando somente produtos ativos.
- Carrinho com quantidades inteiras e totalizador.
- Histórico, repetição e acompanhamento de pedidos.
- Entrega somente em Foz do Iguaçu–PR.
- Taxa de entrega opcional, configurada pelo administrador.
- PIX Sicoob com QR Code, Copia e Cola e confirmação automática.
- Cartão e dinheiro pagos na entrega.
- Uma única conta administrativa.
- Salvamento local de rascunhos e dados consultados.

## 3. Arquitetura proposta

| Projeto | Responsabilidade |
|---|---|
| VanderLimp.App | Aplicativo .NET MAUI |
| VanderLimp.Api | API ASP.NET Core, regras e integrações |
| VanderLimp.Admin | Painel web Blazor |
| VanderLimp.Contracts | Contratos de comunicação com a API |

MAUI, MySQL e o acesso administrativo pelo computador estão definidos. ASP.NET Core, Blazor, SQLite e a divisão em quatro projetos compõem a proposta técnica apresentada.

O aplicativo e o painel acessam a API. Somente a API acessa o MySQL e a integração Sicoob. Credenciais bancárias e do banco de dados não ficam no aplicativo nem no repositório.

### Organização do aplicativo

| Pasta | Conteúdo |
|---|---|
| Models | Dados locais |
| Views | Telas XAML |
| ViewModels | Comportamento das telas, seguindo MVVM |
| Services | API e sessão |
| Data | Persistência SQLite |
| Resources | Imagens, estilos, fontes e cores |

### Persistência

| MySQL — fonte oficial | SQLite — aparelho |
|---|---|
| Clientes e endereços | Dados necessários do cliente |
| Produtos e preços | Último catálogo consultado |
| Pedidos e itens | Carrinho em montagem |
| Pagamentos e cobranças | Cópias dos pedidos consultados |
| Histórico e configurações | Controle de sincronização |

A sessão ficará no armazenamento seguro do aparelho. Senhas não serão salvas em texto simples.

## 4. Cadastro e acesso do cliente

| Campo | PF | PJ | Obrigatório |
|---|---|---|---|
| Nome completo | Sim | — | Sim |
| CPF | Sim | — | Sim |
| Razão social | — | Sim | Sim |
| CNPJ | — | Sim | Sim |
| Nome fantasia | — | Sim | Não |
| Nome do responsável | — | Sim | Sim |
| Telefone/WhatsApp | Sim | Sim | Sim |
| E-mail | Sim | Sim | Sim |
| Senha e confirmação | Sim | Sim | Sim |

Não exigir CPF do responsável pela empresa é uma proposta para a primeira versão.

Endereço cadastral: CEP, logradouro, número ou indicação de sem número, complemento opcional, bairro, cidade e UF.

Fluxo: preencher dados → preencher endereço → validar pela API → registrar cliente → autenticar → abrir Meus pedidos.

- CPF/CNPJ e e-mail duplicados serão verificados.
- O cadastro será liberado após validação e registro, sem aprovação manual.
- Sem internet, os dados poderão ficar como rascunho; o cadastro não será considerado concluído.
- Acesso automático exige sessão válida, não apenas dados locais.
- Conta existente poderá ser acessada em outro aparelho.
- Haverá recuperação e alteração de senha.

## 5. Produtos

| Campo | Regra |
|---|---|
| Código | Único e obrigatório |
| Descrição | Obrigatória |
| Categoria | Obrigatória |
| Marca | Opcional |
| Unidade de venda | Fixa: UN |
| Conteúdo/tamanho | Quando aplicável |
| Unidade de medida do conteúdo | mL, L, g, kg etc., conforme conteúdo |
| Preço | Por UN, maior que zero |
| Situação | Ativo ou inativo |

Todos os produtos serão vendidos por UN, com quantidades inteiras. Não haverá venda a granel nem por quilograma. Conteúdo de 1 kg identifica o tamanho do produto, não a unidade de venda.

O campo Tipo de embalagem foi retirado.

Apresentações distintas terão cadastros separados: detergente 500 mL e detergente 5 L, por exemplo.

- Inativação retira o produto do catálogo atualizado.
- Pedidos antigos preservam descrição e preço da compra.
- Alterações de preço registram data e responsável.
- Categorias e marcas têm nome e situação.
- Evitar nomes duplicados desconsiderando caixa e espaços extras.
- Inativar categoria ou marca preserva vínculos e não inativa automaticamente produtos.

## 6. Carrinho e totalizador

- Quantidade inicial: 1.
- Adicionar novamente o mesmo produto aumenta a quantidade.
- Controles de quantidade e digitação direta.
- Somente números inteiros positivos.
- Subtotal do item = quantidade × preço unitário.
- Subtotal dos produtos = soma dos subtotais.
- Total = subtotal dos produtos + taxa de entrega.
- Recalcular imediatamente após inclusão, remoção ou alteração.
- Barra fixa no catálogo com produtos diferentes, subtotal e acesso ao carrinho.
- Mostrar entrega e total quando a taxa estiver definida.
- Se a taxa não estiver definida, mostrar Entrega a confirmar; não apresentar subtotal como total definitivo.
- Carrinho vazio impede avanço.
- Rascunho salvo após alterações e preservado ao fechar o aplicativo.
- Antes de substituir carrinho existente, solicitar confirmação.

A API revalida preços, quantidades, produtos ativos e taxa na finalização. Alterações exigem revisão do cliente.

## 7. Entrega

- Somente Foz do Iguaçu–PR, validado pela API.
- Endereço cadastral pode ser de outra cidade.
- Um endereço de entrega por pedido.
- Usar endereço cadastral, salvo ou novo.
- Endereço novo pode ser usado somente naquele pedido ou salvo.
- Campos: CEP, logradouro, número/sem número, complemento, bairro, cidade, UF e referência opcional.
- Observações de entrega opcionais.
- Alterar endereço de entrega não altera cadastro.
- Pedido guarda uma cópia do endereço confirmado.

### Configuração simples

Administrador poderá definir Cobrar entrega? (Sim/Não) e Valor da taxa. Taxa única, sem cálculo por distância ou bairro na primeira versão.

Quando desabilitada: Entrega grátis e taxa zero. Mudanças valem para novos pedidos; pedidos confirmados preservam o valor registrado.

Não haverá programação automática de promoções nesta versão.

## 8. Pedidos e status

### Registro

Rascunho → aguardando envio → recebido pelo servidor.

Somente após resposta da API o aplicativo mostra Pedido recebido. Identificador único de envio evita duplicidade em tentativas repetidas.

Itens guardam quantidade e preço da compra. O pedido preserva total, entrega e endereço.

### Entrega

Recebido → Em separação → Enviado → Entregue.

Cancelado permanece no histórico com motivo.

### Pagamento

Controlado separadamente: aguardando pagamento, pago, pagamento na entrega, expirado e reembolsado, conforme o fluxo aplicável.

- PIX pendente não libera separação.
- Cartão e dinheiro na entrega permitem separação.
- Entregue não implica pago automaticamente.
- Repetir pedido cria novo carrinho com preços atuais e revisão de disponibilidade.
- Cliente acessa somente seus próprios pedidos.
- Proposta inicial: cancelamento solicitado à empresa e registrado pela administração.
- Cancelamento não significa reembolso automático.
- Correções de itens, preços ou endereço após confirmação exigirão fluxo específico, ainda não definido.

## 9. Pagamentos

### PIX Sicoob

1. API valida e registra o pedido.
2. Solicita cobrança ao Sicoob.
3. Aplicativo apresenta número, valor, QR Code, Copia e Cola e validade.
4. Cliente paga pelo banco de sua preferência.
5. Integração comunica confirmação à API.
6. API verifica identificação e valor.
7. Pagamento passa a Pago e libera separação.

- Consultar pagamento verifica a cobrança, sem confirmação manual.
- Fechar aplicativo não impede confirmação no servidor.
- Falha na geração permite nova tentativa no mesmo pedido.
- Cobrança expirada pode ser renovada após consulta e revalidação.
- Mudanças de total exigem confirmação antes de nova cobrança.
- Pedido pago não gera nova cobrança.
- Requisitos reais de acesso à API Sicoob e prazo do PIX ainda serão definidos.
- QR Code isolado não garante confirmação automática.

### Cartão e dinheiro

Pagos na entrega e registrados pelo administrador.

Dinheiro poderá indicar necessidade de troco e valor entregue. Esse valor deverá ser igual ou maior que o total.

Registro distingue valor devido, dinheiro entregue e troco. A forma efetivamente recebida poderá diferir da escolhida inicialmente.

Impedir recebimentos duplicados. Correções exigem justificativa e histórico. Não há processamento de cartão on-line no escopo.

## 10. Telas do aplicativo

| Tela | Principais funções |
|---|---|
| Primeiro acesso | Criar cadastro / Já tenho cadastro |
| Cadastro | Dados PF/PJ e acesso |
| Endereço cadastral | Completar e finalizar cadastro |
| Acesso | Entrar / Esqueci minha senha |
| Meus pedidos | Novo pedido, último pedido, histórico e rascunho |
| Produtos | Pesquisa, filtros, quantidade, adicionar e totalizador |
| Carrinho | Ajustar, remover, continuar compras e escolher entrega |
| Entrega | Selecionar endereço e observações |
| Revisão e pagamento | Resumo final, forma e confirmação |
| Pagamento PIX | QR Code, Copia e Cola, validade e consulta |
| Detalhes do pedido | Itens, valores, pagamento e etapas |
| Minha conta | Dados, endereços, senha e sair |

Navegação inferior: Pedidos, Produtos e Minha conta.

### Meus pedidos e detalhes

Último pedido em destaque; histórico do mais recente ao mais antigo. Mostrar número, data, total, pagamento e entrega. PIX pendente oferece Pagar com PIX.

### Minha conta

- Editar contato e endereço; para PJ, nome fantasia e responsável.
- Alteração de CPF/CNPJ ou tipo de pessoa via administração.
- Troca de e-mail mediante confirmação do novo endereço.
- Endereços podem receber nomes e um padrão.
- Exclusão de endereço não apaga histórico.
- Sair encerra sessão e retira dados pessoais da visualização local.
- Avisar antes de descartar rascunho ao sair.

## 11. Painel administrativo

Uma única conta administrativa com e-mail, senha e recuperação de acesso. Cadastro de cliente não concede acesso administrativo.

| Tela | Funções |
|---|---|
| Acesso | Autenticação administrativa |
| Visão geral | Novos pedidos, PIX pendentes, separação, enviados e entregues hoje |
| Pedidos | Pesquisa, filtros, detalhes e impressão |
| Produtos | Cadastro, edição, preço e ativação/inativação |
| Categorias e marcas | Organização do catálogo |
| Clientes | Consulta, correções, endereços, compras e bloqueio |
| Pagamentos | PIX e registro de recebimento na entrega |
| Configurações | Cobrar entrega e valor único |

### Pedidos

Pesquisa por número, cliente ou documento; filtros por período, entrega, pagamento e forma.

Ações: iniciar separação, marcar enviado, marcar entregue, registrar recebimento, cancelar com motivo e imprimir.

Registrar data, horário e conta responsável pelas alterações.

### Clientes

Abas Cadastro, Endereços de entrega e Pedidos. Bloqueio exige motivo e impede novas compras; mantém consulta do histórico e pagamento de pedido pendente. Não mostrar senhas.

### Pagamentos

Consulta por pedido/cliente, período, forma e situação. PIX confirmado pela integração; dinheiro/cartão registrados na entrega. Reembolso separado de cancelamento.

## 12. Sincronização e segurança

- Sem internet: consultar dados salvos e montar rascunhos.
- Exibir aviso de dados possivelmente desatualizados.
- Exigir conexão para concluir cadastro e confirmar pedido.
- Atualizar catálogo, pedidos e pagamentos pela API.
- Validar autorização e regras no servidor.
- Comunicação protegida por HTTPS.
- Guardar senhas com hash apropriado no servidor.
- Segredos fora do repositório; usar configuração segura do ambiente.
- Registrar falhas e alterações sem expor senhas ou credenciais.
- Definir backups e recuperação antes da operação real.

## 13. Ordem de desenvolvimento

1. Finalizar modelo de dados e relacionamentos.
2. Criar solução, projetos, MySQL e persistência.
3. Cadastro, autenticação e conta administrativa.
4. Produtos, categorias e marcas.
5. Catálogo, carrinho e totalizador.
6. Entrega e confirmação de pedidos.
7. Histórico, repetição, acompanhamento e recebimento.
8. Integração PIX Sicoob.
9. Verificação completa, sincronização e falhas de conexão.
10. Revisar funcionalidades adiadas com o responsável.

As tabelas finais, tecnologias em versões específicas, implantação e código ainda não foram definidos.

## 14. Pontos pendentes

- Validar proposta técnica e versões compatíveis.
- Definir plataformas iniciais do MAUI.
- Definir hospedagem da API, painel e MySQL.
- Confirmar acesso e requisitos da integração PIX Sicoob.
- Definir validade de cobrança e tratamento de confirmação tardia.
- Detalhar cancelamento, reembolso e correções de pedidos.
- Definir serviço de e-mail para confirmação e recuperação.
- Definir operação de entrega e backups.
- Projetar tabelas e relacionamentos no próximo passo.

## 15. Funcionalidades adiadas — revisão obrigatória no final

| Tela | Adiado |
|---|---|
| Entrega e promoções | Programação de períodos promocionais e entrega gratuita automática |
| Funcionários e permissões | Cadastro de funcionários e perfis de acesso |

A configuração manual da taxa e a conta administrativa única permanecem no escopo.

### Checklist de encerramento

- [ ] Apresentar todas as telas adiadas ao responsável.
- [ ] Perguntar se deseja incluir Entrega e promoções.
- [ ] Perguntar se deseja incluir Funcionários e permissões.
- [ ] Registrar decisões e atualizar este planejamento.
- [ ] Verificar cadastro, catálogo, carrinho, pedido, pagamento e entrega.
- [ ] Validar falhas de conexão e prevenção de duplicidades.

## 16. Uso no GitHub

Este documento pode ser adicionado ao repositório como README.md ou mantido como PLANEJAMENTO_VANDER_LIMP.md.

É uma especificação de planejamento; não representa funcionalidades já implementadas.

