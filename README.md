# netcore-authentication-keycloak

Poc utilizando .net core 3.1 para o uso de autorização através do [Keycloak](https://www.keycloak.org/ "Keycloak").

Iremos simular uma aplicação que faz a gestão backoffice de um marketplace. Nesse cenário, precisamos proteger nossos endpoints e além disso, validar se o usuário está "abaixo do guarda-chuva" do recurso que pretente utilizar. Para isso, trabalharemos com um conceito de **Multitenancy**.

Para fins didáticos, o código está o mais enxuto e simples possível. A ideia principal é focarmos no tema de autenticação/autorização.

## Hands on

Primeiramente precisamos realizar a criação da aplicação net core. Podemos realizar essa operação a partir do seguinte comando:

```cmd
dotnet new webapi -n Marktplace.Backoffice
```

O resultado deve ser algo como a imagem abaixo:
![dotnet new result](https://github.com/allysonjoses/netcore-authentication-keycloak/blob/main/docs/images/1-dotnet-new-webapi.png?raw=true)

O dotnet new acabou de criar um projeto ASP.NET Core Web API dentro da pasta Marketplace.Backoffice, a partir de um template. Para ver todos os templates disponíveis você pode executar o seguinte comando:

```cmd
dotnet new --help
```
Para certificar que está tudo ok, iremos realizar um restore e build :

```cmd
cd .\Marktplace.Backoffice\
dotnet restore
dotnet build
```

### Cara, crachá

Nessa aplicação, utilizaremos o [Keycloak](https://www.keycloak.org/ "Keycloak") como provedor de indentidade (IdP). Consequentemente ele será o responsável por autenticar nossos usuários e ou aplicações.

Para gente, nos resta a obrigação de validar os tokens de acessos gerados pelo Keycloak e autorizar o acesso as informações protegidas do nosso domínio (existem inumeras formas para isso, ex: claims, scopes, roles etc).

Para isso, iremos instalar a dependência necessária para trabalharmos com tokens JTW.

```cmd
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer -v 3.1.12
```
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

6. `ValidAudience `
 TODO

7. `ValidIssuer`
 TODO
