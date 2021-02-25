# netcore-authentication-keycloak

Poc utilizando .net core 3.1 para o uso de autorização através do Keycloak.

Iremos simular uma aplicação que faz a gestão backoffice de um marketplace. Nesse cenário, precisamos proteger nossos endpoints e além disso, validar se o usuário está "abaixo do guarda-chuva" do recurso que pretente utilizar. Para isso, trabalharemos com um conceito de **Multitenancy**.

Para fins didáticos, o código está o mais enxuto e simples possível. A ideia principal é focarmos no tema de autenticação/autorização.

## Hands on

Primeiramente precisamos realizar a criação da aplicação net core. Podemos realizar essa operação a partir do seguinte comando:

```cmd
dotnet new webapi -n Marktplace.Backoffice
```

O resultado deve ser algo como o seguinte:
