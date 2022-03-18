# netcore-authentication-keycloak

Poc utilizando .net core 3.1 para o uso de autorização através do [Keycloak](https://www.keycloak.org/ "Keycloak").

Iremos simular uma aplicação que faz a gestão backoffice de um marketplace. Nesse cenário, precisamos proteger nossos endpoints e além disso, validar se o usuário está "abaixo do guarda-chuva" do recurso que pretende utilizar. Para isso, trabalharemos com um conceito de **Multitenancy**.

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

Nessa aplicação, utilizaremos o [Keycloak](https://www.keycloak.org/) como provedor de identidade (IdP). Consequentemente ele será o responsável por autenticar nossos usuários e ou aplicações. Vou deixar aqui algumas ilustrações que facilitará o nosso entendimento:
- [Authorizations flows types](https://github.com/allysonjoses/netcore-authentication-keycloak/blob/main/docs/images/2-authorization-flows-overview.jpg?raw=true)
- [Authorization Code Flow detail (micro)](https://github.com/allysonjoses/netcore-authentication-keycloak/blob/main/docs/images/2.1-authorization-code-flow.jpg?raw=true)
- [Authorization Code Grant Flow detail (macro)](https://github.com/allysonjoses/netcore-authentication-keycloak/blob/main/docs/images/2.3-authorization-code-illustration.jpeg?raw=true)
- [Client Credential Flow detail (macro)](https://github.com/allysonjoses/netcore-authentication-keycloak/blob/main/docs/images/2.2-client_flow-illustration.jpeg?raw=true)

Para gente, nos resta a obrigação de validar os tokens de acessos gerados pelo Keycloak e autorizar o acesso às informações protegidas do nosso domínio (existem inúmeras formas para isso, ex: claims, scopes, roles etc).

Para isso, iremos instalar a dependência necessária para trabalharmos com tokens JWT.

`
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer -v 3.1.0
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

Essa é toda a configuração necessária que precisamos para receber e validar tokens JWT nos headers de nossas requisições. Vamos por partes:

- `AddAuthentication(...)`: Utilizaremos esse método para definir nossa autenticação. No nosso caso iremos utilizar o Jwt Bearer.

- `AddJwtBearer(...)`: Após informado a forma de autenticação no passo anterior, iremos configurar as opções do JWT bearer.

- `Authority`: é o endereço do servidor de autenticação que emite o token. O middleware de portador do JWT usa esse URI para obter a chave pública que pode ser usada para validar a assinatura do token. O middleware também confirma se o parâmetro iss no token corresponde a esse URI

- `Audience`: representa o receptor do token ou do recurso de entrada ao qual o token concede acesso. Se o valor especificado nesse parâmetro não corresponder ao parâmetro no token, o token será rejeitado

- `Configuration["Authentication: ..."]`: Estamos parametrizando as entradas, com o intuito de facilitar nossas configurações! para isso, precisamos adicionar o seguinte bloco no arquivo `appsettings.json`:

```json
  "Authentication": {
    "RequireHttpsMetadata": false,
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

- `options.TokenValidationParameters`: Objeto que contem  todas as opções de configuração de validação do nosso token

- `ValidIssuer`: Endereço do emissor do token.

Com este middleware em vigor, os tokens JWT são extraídos automaticamente dos cabeçalhos de autorização. Eles são desserializados, validados (usando os valores nos parâmetros Audience e Authority) e armazenados como informações de usuário a serem referenciadas posteriormente por ações de MVC ou filtros de autorização.

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

Com isso, será baixado a imagem do keycloak, criando-se assim, um container com o nome keycloak-server, o qual será executado localmente na porta 8080. Acesse http://localhost:8080/auth/admin para entrarmos na área logada. O login e senha são os valores definidos nas linhas 10 e 11 do nosso docker-compose.

O Keycloak utiliza um sistema de realms para gerenciar conjuntos de usuários, credenciais, funções e grupos. Um usuário pertence e efetua login em um realm. Os realms são isolados uns dos outros e só podem gerenciar e autenticar os usuários que controlam.

Quando logamos pela primeira vez em um servidor Keycloak entramos em um realm chamado master, esse realm é utilizado para adicionamos usuários com acesso administrativo ao servidor. Se pensarmos em nível de hierarquia, esse realm estaria acima dos demais.

No menu inicial, vale destacar os seguintes itens:

![Keycloak](https://github.com/allysonjoses/netcore-authentication-keycloak/blob/main/docs/images/3-menu_kc.png?raw=true)

- Master – O realm em que estamos trabalhando. É fortemente recomendado que antes configurar nossa aplicação, seja criado antes um novo realm para os usuários que vão acessar o sistema.  Para criar esse novo realm, coloque o mouse sobre o Master, e clique em “Add realm” e crie um realm chamado "demo".

- Realms Settings – Página de configuração do realm. Aqui podemos configurar o nome do realm, as informações que serão mostrado ao usuário na tela de login, configurar o provedor de email para confirmação de usuário, entre outras coisas.

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

##### Client Credentials flow

Para que seja possível o cenário API to API, precisamos criar um client para a aplicação que acessará nossa api. Crie um novo client com o seguinte Client ID: `mktp-riachuelo-integration`.

Iremos realizar as seguintes mudanças em nosso client:

- Access Type: definir como confidential. (Secrets é necessário para o login)
- Standard Flow Enabled: definir como OFF.
- Implicit Flow Enabled: definir como OFF.
- Direct Access Grants Enabled: definir como OFF.
- Service Accounts Enabled: definir como ON
- Authorization Enabled: definir como ON.
- Clique em salvar.

Iremos utilizar o Client Credential Grant Flow para gerar o token de acesso da aplicação `mktp-riachuelo-integration`. Segue o curl que precisamos utilizar:

```bash
curl --location --request POST 'http://localhost:8080/auth/realms/demo/protocol/openid-connect/token' \
--header 'Authorization: Basic bWt0cC1yaWFjaHVlbG8taW50ZWdyYXRpb246NjNhYzYyNzktZGM1MC00ZmEyLWIyOGYtNTk2ZjFkNGVmZDJl' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'grant_type=client_credentials'
```

Destrinchando essa requisição temos:

- URL: Essa url pode ser encontrada em: `Realm Settings` (Menu principal) -> `Endpoints: OpenID Endpoint Configuration` ou acessar o seguinte link: <http://localhost:8080/auth/realms/demo/.well-known/openid-configuration>. Nele encontraremos o token_endpoint que é url que precisamos utilizar para gerar nosso access token.
- Authorization header: Precisamos do header Authorization do tipo basic, no seguinte formato: `Authorization: Basic {value}`. O value é composto pelo ClientID:Secrets do seu client, encodado em base 64. No nosso caso seria: `mktp-riachuelo-integration:63ac6279-dc50-4fa2-b28f-596f1d4efd2e` (não esquecer de encondar). O secrets do seu client será diferente deste do exemplo, para obte-lo, acesse o seu client e vá na aba `Credentials`.

O Response da requisição deve ser algo parecido com isso:

```json
{
"access_token": "eyJhbGciOiJSUzI1NiIsInR5cCIgOiAiSldUIiwia2lkIiA6ICJQYkRQQlRYV1JINW9CUUhZX0dieWNOLUlBSGlkOXllUHl6RDlTMU1pN3JjIn0.eyJleHAiOjE2MTQzMDY5NzEsImlhdCI6MTYxNDMwNjY3MSwianRpIjoiMWVhNjI0OWYtMDY0Yy00ZGIyLWE0ODAtNDEwMTU3ODY4YTE1IiwiaXNzIjoiaHR0cDovL2xvY2FsaG9zdDo4MDgwL2F1dGgvcmVhbG1zL2RlbW8iLCJhdWQiOiJhY2NvdW50Iiwic3ViIjoiZDE2Y2YwNDktNzcyZC00MTMxLTg2YTgtM2U3ZjNlZWI3Nzk3IiwidHlwIjoiQmVhcmVyIiwiYXpwIjoibWt0cC1yaWFjaHVlbG8taW50ZWdyYXRpb24iLCJhY3IiOiIxIiwicmVhbG1fYWNjZXNzIjp7InJvbGVzIjpbIm9mZmxpbmVfYWNjZXNzIiwidW1hX2F1dGhvcml6YXRpb24iXX0sInJlc291cmNlX2FjY2VzcyI6eyJta3RwLXJpYWNodWVsby1pbnRlZ3JhdGlvbiI6eyJyb2xlcyI6WyJ1bWFfcHJvdGVjdGlvbiJdfSwiYWNjb3VudCI6eyJyb2xlcyI6WyJtYW5hZ2UtYWNjb3VudCIsIm1hbmFnZS1hY2NvdW50LWxpbmtzIiwidmlldy1wcm9maWxlIl19fSwic2NvcGUiOiJlbWFpbCBwcm9maWxlIiwiY2xpZW50SWQiOiJta3RwLXJpYWNodWVsby1pbnRlZ3JhdGlvbiIsImVtYWlsX3ZlcmlmaWVkIjpmYWxzZSwiY2xpZW50SG9zdCI6IjE3Mi4xOC4wLjEiLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJzZXJ2aWNlLWFjY291bnQtbWt0cC1yaWFjaHVlbG8taW50ZWdyYXRpb24iLCJjbGllbnRBZGRyZXNzIjoiMTcyLjE4LjAuMSJ9.IrATSri0ZEbrYhuH5kYZBq2edscczAdKf9HOksNMh2yEJLK2oZaka--Kf4dujS0JnwM1_LaVz3Od1WBBjCh8egjhd0vk-uPAC9_ev76CAS6Lbqni8kMBbXefxQn2oBf98uXnq4Dy1xK97xWyhTDvfr1XkeFL2yAT1nyDOKyIezp9jA0kIdmXDh69vnd3YuhTcE8adCbZmODPHXmZKJaMHH8ZOBNFLe2TNCrE-geioh3r-oN6HkmEe4GPTA3HuJTfKtOkGR5I8c5SWgpiJloqbpNw9jHv-nCxZ9TMibXprqTImLFGDmze1uLyPc5V5IU5EMBN6Iy2SZ6pMsGghbV3rA",
  "expires_in": 300,
  "refresh_expires_in": 0,
  "token_type": "Bearer",
  "not-before-policy": 0,
  "scope": "email profile"
}
```

Vamos usar o site <https://jwt.io> para visualizar o nosso access token:

```json
{
  "exp": 1614306971,
  "iat": 1614306671,
  "jti": "1ea6249f-064c-4db2-a480-410157868a15",
  "iss": "http://localhost:8080/auth/realms/demo",
  "aud": "account",
  "sub": "d16cf049-772d-4131-86a8-3e7f3eeb7797",
  "typ": "Bearer",
  "azp": "mktp-riachuelo-integration",
  "acr": "1",
  "realm_access": {
    "roles": [
      "offline_access",
      "uma_authorization"
    ]
  },
  "resource_access": {
    "mktp-riachuelo-integration": {
      "roles": [
        "uma_protection"
      ]
    },
    "account": {
      "roles": [
        "manage-account",
        "manage-account-links",
        "view-profile"
      ]
    }
  },
  "scope": "email profile",
  "clientId": "mktp-riachuelo-integration",
  "email_verified": false,
  "clientHost": "172.18.0.1",
  "preferred_username": "service-account-mktp-riachuelo-integration",
  "clientAddress": "172.18.0.1"
}
```

Agora precisamos conceder acesso a role que criamos no `client mktp-backoffice-api` para o client `mktp-riachuelo-integration`. Para isso, abra o client `mktp-riachuelo-integration` e em seguida a aba `Service Account Roles`.

![Service Account Roles](https://github.com/allysonjoses/netcore-authentication-keycloak/blob/main/docs/images/4-service-account-roles.png?raw=true)

No select `Client Roles`, selecione `mktp-backoffice-api` e em seguida selecione a role `view-seller` e clique em `Add selected`.

![Service Account Roles](https://github.com/allysonjoses/netcore-authentication-keycloak/blob/main/docs/images/4.1-service-account-roles-add-role.png?raw=true)

Feito isso, realize novamente a chamada de obtenção do token e em seguida abra-o no https://jwt.io. Veja como ficou o nosso token:

```json
{
  "exp": 1614311770,
  "iat": 1614311470,
  "jti": "3ceeb6c7-f79f-4034-9864-f6db74d78345",
  "iss": "http://default-host:8080/auth/realms/demo",
  "aud": [
    "mktp-backoffice-api",
    "account"
  ],
  "sub": "d16cf049-772d-4131-86a8-3e7f3eeb7797",
  "typ": "Bearer",
  "azp": "mktp-riachuelo-integration",
  "acr": "1",
  "realm_access": {
    "roles": [
      "offline_access",
      "uma_authorization"
    ]
  },
  "resource_access": {
    "mktp-riachuelo-integration": {
      "roles": [
        "uma_protection"
      ]
    },
    "mktp-backoffice-api": {
      "roles": [
        "view-seller"
      ]
    },
    "account": {
      "roles": [
        "manage-account",
        "manage-account-links",
        "view-profile"
      ]
    }
  },
  "scope": "email profile",
  "clientId": "mktp-riachuelo-integration",
  "email_verified": false,
  "clientHost": "172.18.0.1",
  "preferred_username": "service-account-mktp-riachuelo-integration",
  "clientAddress": "172.18.0.1"
}
```

Observe o conteúdo das **linhas 26 a 30**. Agora temos a presença do role do `client mktp-backoffice-api` em nosso token. Atenção para o formato default que o Keycloak utiliza para expor as roles, iremos conversar sobre ele em breve!

##### Authorization Code flow

Agora que já sabemos como trabalhar para conceder acesso de uma aplicação, para outra aplicação, precisamos fazer o mesmo para os usuários.

Clique na opção de menu `Users` e em seguida em `Add user` (canto direito da tela). Na tela de criação de Users, preencha as informações do seu usuário e deixe como ON as opções `Email Verifie`d e `User Enabled`.

Você pode definir a senha do seu usuário, ou enviar um e-mail de reset de senha na aba Credentials (O SMTP precisaria está configurado).

Podemos conceder acesso ao recurso `view-seller` de nossa api de backoffice de duas maneiras: - Através da aba `Role Mappings` (a dinamica funciona igual o Service Account Roles do client, primeiro, selecione o client e em seguida as roles e clique em add selected).
- Através da aba `Groups`, aqui você precisará possuir um grupo que esteja associado as roles que você necessite. Em `Available Groups`, clique em um dos grupos listados e posteriormente em `Join`. (Você pode criar e gerenciar grupos através no menu principal `Groups`).

A forma como iremos autenticar nossos usuários muda um pouco em comparação a api. Na api, não precisamos de uma interface, a proópria aplicação obtem o token através de uma chamada rest, entretanto, não podemos exigir o mesmo de um usuário! Para ele precisamos disponibilizar uma tela de login, mas não se preocupe, o Keycloak já nos dá uma pronta (E você pode personaliza-la como quiser).

Imagine que o nossa api de backoffice terá também um frontend, o qual irá disponilizar para o usuário todas as features que possuimos em nossa api. Dessa forma, o pensamento padrão seria a utilização do client que já possuímos no Keycloak, o `mktp-backoffice-api` para servir como client também para o frontend, porém, isso não é nada recomendando! 

O frontend é uma aplicação "insegura", logo não é possível armazenar o secrets do nosso client com segurança em nosso frontend! Por isso, temos um tipo de `Client Access Type` específico para esse cenário; o `public`. Como ele, não necessitamos do secrets para iniciar o login! Para isso, necessitamos também trabalhar com outro Authorization Flow, o `Implicit` ou `Authorization Code`. Recomendo a utilização do `Authorization Code` com [`PKCE`](https://oauth.net/2/pkce/).

Vamos criar um client para a aplicação de frontend. Crie um novo client com o seguinte Client ID: `mktp-backoffice-frontend`.

Iremos realizar as seguintes mudanças em nosso client:

- Access Type: definir como public.
- Standard Flow Enabled: definir como ON.
- Implicit Flow Enabled: definir como OFF.
- Direct Access Grants Enabled: definir como OFF.
- Valid Redirect URIs: definir como `*`. (Atenção, é de suma importância a configuração correta das uris de redirect permitidas pela sua aplicação! Usaremos * apenas para fins didáticos).
- Web Origins: definir como `*`. (Atenção, é de suma importância a configuração correta por sua aplicação! Usaremos * apenas para fins didáticos).
- Clique em salvar.

Agora, seremos capazes de autenticar o usuário via browser! Nosso frontend irá implementar o Authorizantion Code e quando o usuário não estiver autenticado, o mesmo será redirecionado para:

http://localhost:8080/auth/realms/demo/protocol/openid-connect/auth?response_type=code&state=&client_id=mktp-backoffice-frontend&scope=profile&redirect_uri=http%3A%2F%2Flocalhost%3A3000

O usuário inputará seu login e senha e em seguida, será redirecionado de volta para a sua aplicação (`redirect_uri`) com o query parameter `code`. O frontend irá trocar assim, esse code pelo token do usuário realizando uma chamada no endpoint de token:

```bash
curl --location --request POST 'http://localhost:8080/auth/realms/demo/protocol/openid-connect/token' \
--header 'Authorization: Basic bWt0cC1iYWNrb2ZmaWNlLWZyb250ZW5kOg==' \
--header 'Content-Type: application/x-www-form-urlencoded' \
--data-urlencode 'grant_type=authorization_code' \
--data-urlencode 'code=f7112040-f8e5-420e-9942-2af35fc6e27d.92506486-2d64-4f46-9728-d3efc7839564.9c3945cd-38e8-4ed5-8bb2-8c1616569e59' \
--data-urlencode 'redirect_uri=http://localhost:3000' \
--data-urlencode 'client_id=mktp-backoffice-frontend'
```

Notem que agora, diferentemente do flow de `Client Credentials`, não precisamos da figura do secrets para gerar o token! Pode parecer um pouco complexo de início, porém, existem diversas bibliotecas disponíveis realizam o "trabalho sujo" no frontend, nos economizando tanto o tempo de implementação da tela de login, assim como, diminuindo a complexidade de utilizar o Authorization Code!

#### De volta para a API

Agora que somos capazes de gerar tokens, já podemos utilizalos em nossa aplicação! Primeiramente, precisamos atualizar nosso arquivo appsettings.json:

```json
 "Authentication": {
    "RequireHttpsMetadata": false,
    "Authority": "http://localhost:8080/auth/realms/demo",
    "IncludeErrorDetails": true,
    "ValidateAudience": true,
    "ValidAudience": "mktp-backoffice-api",
    "ValidateIssuerSigningKey": true,
    "ValidateIssuer": true,
    "ValidIssuer": "http://localhost:8080/auth/realms/demo",
    "ValidateLifetime": true
  }
```

Notem que definimos os seguintes campos: `Authority`, `ValidAudience`, `ValidIssuer`.

Agora criaremos um Controller novo, chamado SellerController
Criar um controller com endpoints protegidos

...
Tudo certo? Ainda não... Lembram do formato default que o Keycloak usa na claim que lista as roles? Pois é, por default, o dotnet não trabalha com o mesmo formato ... (TODO)

Trabalhar o conceito de Multitenancy (TODO)
