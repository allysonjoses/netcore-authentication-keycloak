# netcore-authentication-keycloak

Poc utilizando .net core 3.1 para o uso de autorização através do [Keycloak](https://www.keycloak.org/ "Keycloak").

Iremos simular uma aplicação que faz a gestão backoffice de um marketplace. Nesse cenário, precisamos proteger nossos endpoints e além disso, validar se o usuário está "abaixo do guarda-chuva" do recurso que pretente utilizar. Para isso, trabalharemos com um conceito de **Multitenancy**.

Para fins didáticos, o código está o mais enxuto e simples possível. A ideia principal é focarmos no tema de autenticação/autorização.

## Hands on

Primeiramente precisamos realizar a criação da aplicação net core. Podemos realizar essa operação a partir do seguinte comando:

`dotnet new webapi -n Marktplace.Backoffice`

O resultado deve ser algo como a imagem abaixo:
![dotnet new result](https://github.com/allysonjoses/netcore-authentication-keycloak/blob/main/docs/images/1-dotnet-new-webapi.png?raw=true)

O dotnet new acabou de criar um projeto ASP.NET Core Web API dentro da pasta Marketplace.Backoffice, a partir de um template. Para ver todos os templates disponíveis você pode executar o seguinte comando:

`dotnet new --help`

Para certificar que está tudo ok, iremos realizar um restore e build :

`cd .\Marktplace.Backoffice\`

`dotnet restore`

`dotnet build`
`
### Cara, crachá

Nessa aplicação, utilizaremos o [Keycloak](https://www.keycloak.org/) como provedor de indentidade (IdP). Consequentemente ele será o responsável por autenticar nossos usuários e ou aplicações. Vou deixar aqui algumas ilustrações que facilitará o nosso entendimento:
- [Authorizations flows types](https://github.com/allysonjoses/netcore-authentication-keycloak/blob/main/docs/images/2-authorization-flows-overview.jpg?raw=true)
- [Authorization Code Flow detail (micro)](https://github.com/allysonjoses/netcore-authentication-keycloak/blob/main/docs/images/2.1-authorization-code-flow.jpg?raw=true)
- [Authorization Code Grant Flow detail (macro)](https://github.com/allysonjoses/netcore-authentication-keycloak/blob/main/docs/images/2.3-authorization-code-illustration.jpeg?raw=true)
- [Client Credential Flow detail (macro)](https://github.com/allysonjoses/netcore-authentication-keycloak/blob/main/docs/images/2.2-client_flow-illustration.jpeg?raw=true)

Para gente, nos resta a obrigação de validar os tokens de acessos gerados pelo Keycloak e autorizar o acesso as informações protegidas do nosso domínio (existem inumeras formas para isso, ex: claims, scopes, roles etc).

Para isso, iremos instalar a dependência necessária para trabalharmos com tokens JTW.

`
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer -v 3.1.12
`

Agora iremos adicionar os seguintes usings no arquivo `Startup.cs` presente na pasta raiz de nossa aplicação.

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
```

Ainda no `Startup.cs` iremos adicionar a seguinte instrução no método `ConfigureServices`:

```csharp
services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
	options.RequireHttpsMetadata = bool.Parse(Configuration["Authentication:RequireHttpsMetadata"]);
	options.Authority = Configuration["Authentication:Authority"];
	options.IncludeErrorDetails = bool.Parse(Configuration["Authentication:IncludeErrorDetails"]);
	options.TokenValidationParameters = new TokenValidationParameters()
	{
		ValidateAudience = bool.Parse(Configuration["Authentication:ValidateAudience"]),
		ValidAudience = Configuration["Authentication:ValidAudience"],
		ValidateIssuerSigningKey = bool.Parse(Configuration["Authentication:ValidateIssuerSigningKey"]),
		ValidateIssuer = bool.Parse(Configuration["Authentication:ValidateIssuer"]),
		ValidIssuer = Configuration["Authentication:ValidIssuer"],
		ValidateLifetime = bool.Parse(Configuration["Authentication:ValidateLifetime"])
	;
});
```

Isso é toda a configuração necessário que precisamos para sermos capazes de receber tokens JWT nos headers de nossas requisições e validados. Vamos por partes:
1. `AddAuthentication(...)`:
 TODO

2. `AddJwtBearer(...)`:
 TODO

3. `Configuration["Authentication: ..."]`
 Estamos parametrizando as entradas, com o intuito de facilitar nossas configurações! para isso, precisamos adicionar o seguinte bloco no arquivo `appsettings.json`:

```json
  "Authentication": {
    "RequireHttpsMetadata": true,
    "Authority": "your-authority",
    "IncludeErrorDetails": true,
    "ValidateAudience": true,
    "ValidAudience": "your-audience",
    "ValidateIssuerSigningKey": true,
    "ValidateIssuer": true,
    "ValidIssuer": "your-issue",
    "ValidateLifetime":  true
  }
```

4. `options.Authority`
TODO


5. `options.TokenValidationParameters`
 TODO

6. `ValidAudience`
 TODO

7. `ValidIssuer`
 TODO

#### Setup do Keycloak

Precisamos configurar nosso IdP! Para isso utilizaremos o docker-compose para subir o Keycloak localmente e futuramente também a nossa API.

Crie um arquivo chamado `docker-compose.yaml` na raiz do repositório com o seguinte conteúdo:
```yaml
version: '3'

services:
  keycloak:
    image: jboss/keycloak
    container_name: keycloak-server
    ports:
      - "8080:8080"
    environment:
      KEYCLOAK_USER: "rchlo"
      KEYCLOAK_PASSWORD: "123456"
```
Na mesma pasta onde o arquivo foi criado, execute o seguinte comando:

`docker-compose up -d`

Com isso, será baixado a imagem do keycloak, criado uma container com o nome keycloak-server, o qual será executado localmente na porta 8080. Acesse http://localhost:8080/auth/admin para entrarmos na area logada. O login e senha são os valores definidos nas linhas 10 e 11 do nosso docker-compose.

O Keycloak utiliza um sistema de realms para gerenciar conjuntos de usuários, credenciais, funções e grupos. Um usuário pertence e efetua login em um realm. Os realms são isolados uns dos outros e só podem gerenciar e autenticar os usuários que controlam.

Quando logamos pela primeira vez em um servidor Keycloak entramos em um realm chamado master, esse realm é utilizado para adicionamos usuários com acesso administrativo ao servidor. Se pensarmos em nível de hierarquia, esse realm estaria acima dos demais.

No menu inicial, vale destacar os seguintes itens:

- Master – O realm em que estamos trabalhando; é fortemente recomendado que antes configurar nossa aplicação, seja criado antes um novo realm para os usuários que vão acessar o sistema.  Para criar esse novo realm, coloque o mouse sobre o Master, e clique em “Add realm” e crie um realm chamado "demo".

- Realms Settings – Página de configuração do realm, aqui podemos configurar o nome do realm, as informações que serão mostrado ao usuário na tela de login, configurar o provedor de email para confirmação de usuário, entre outras coisas.

- Clients – Clients são as aplicações que usam o keycloak como provedor de autenticação.

- Clients Scopes –  Escopo de cliente que é utilizado para facilitar a criação de novos clientes, compartilhando algumas configurações comuns.

- Identity Providers – Aqui podemos configurar a forma de login usando outras plataformas, tais como redes sociais e também outros relms do keycloak.

- User Federation – Serve para utilizar bases de dados externas na hora de fazer login. Você pode configurar o login com LDAP ou Kerberos.

Nossa api será representada por um client. Nele iremos criar todas roles para proteger os recursos privados do nosso domínio.

Clique na opção de menu `Clients` e em seguida em `Create` (canto direito da tela). Na tela de criação de clients, insira o valor **mktp-backoffice-api** no campo Client ID e na sequência em **Salvar**.

Iremos realizar as seguintes mudanças em nosso client:
- Access Type: definir como bearer-only. (Utilizamos o Access Type como bearer-only quando não precisamos realizar login através desse client)
- Clique em salvar.



Depois disso, iremos criar nossas roles, para isso, clique na aba `Roles` (não confundir com  a opção do menu principal) que fica abaixo do nome do Client no topo da página, posteriormente em Add Role, definindo **view-seller** como nome da role.

Iremos trabalhar com dois cenários de acessos para a nossa api de backoffice.
- API to API: Cenário quando outras apis acessam nossa api.
- User to Api: Quando usuários acessam nossa api.

Para que seja possível o cenário API to API, precisamos criar um client para a aplicação que acessará nossa api. Crie um novo client com o seguinte Client ID: `mktp-riachuelo-integration`.

Iremos realizar as seguintes mudanças em nosso client:
- Access Type: definir como confidential. (Secrets é necessário para o login)
- Standard Flow Enabled: definir como OFF.
- Implicit Flow Enabled: definir como OFF.
- Direct Access Grants Enabled: definir como OFF.
- Service Accounts Enabled: definir como ON
- Authorization Enabled: definir como ON.
- Clique em salvar.

...
  token
  Service Account
  token
  
User
  group
Client frontend
UserToken