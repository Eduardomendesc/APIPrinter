# APIPrinter — ESC/POS Print API (.NET)

API REST em **ASP.NET Core** para enviar comandos **ESC/POS** (em bytes) para uma impressora térmica **via rede** (IP:PORT).  
O endpoint recebe o comando já gerado pelo teu sistema (em **Base64**), converte para bytes e envia diretamente para a impressora.

> Repositório: `Eduardomendesc/APIPrinter` – “ESC-POS-.NET API TO COMUNICATE ESCPOS PRINTER”.
> Licença: MIT.

---

## 📌 Características

- Impressão **via rede** (TCP) usando `ImmediateNetworkPrinter` (biblioteca `ESCPOS_NET`)
- Recebe **comando ESC/POS em Base64** (útil para clientes Web/ERP/POS)
- Retorna resposta JSON simples (`success`, `message`)
- Swagger habilitado em **Development**
- CORS liberado (AllowAll) — ajustar em produção
- Suporte a impressora em rede (IP:PORT) *(e/ou USB/Windows Printer se estiver implementado no projeto)*

---

## 🧱 Stack / Bibliotecas

- ASP.NET Core Web API
- `ESCPOS_NET` (Emitters/Printers/Utilities)
- Swagger / OpenAPI

> Nota: ESC/POS varia por fabricante/modelo. Nem todos comandos funcionam igual em todas impressoras.

---

## ✅ Pré-requisitos

- .NET SDK instalado
- Impressora térmica compatível com ESC/POS
- Impressora acessível pela rede (IP e porta, geralmente **9100**)
- (Opcional) USB/Windows spooler, se o projeto suportar
- Permissões de rede (firewall/liberação da porta)

---

## 🚀 Como executar

### 1) Clonar e restaurar
```bash
git clone https://github.com/Eduardomendesc/APIPrinter.git
cd APIPrinter
dotnet restore
```

### 2) Executar
```bash
dotnet run
```

- Em Development, Swagger normalmente fica em:  
  `https://localhost:<porta>/swagger`

> Dica: se estiveres a usar HTTPS e o cliente for HTTP, valida CORS e certificados. Em ambiente interno, às vezes é mais simples testar via HTTP.

---

## 🌍 CORS

No `Program.cs` existe uma policy **AllowAll**:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

app.UseCors("AllowAll");
```

⚠️ **Produção:** recomenda-se restringir `WithOrigins(...)` para os domínios do teu frontend.

---

## 📡 Endpoint

### POST `/Print`
Controller:
- `[Route("[controller]")]` → `PrintController`
- `[HttpPost]` no método `Print`  
Logo, o endpoint final é:

**`POST /Print`**

---

## 🧾 Payload do request

O corpo do request usa o DTO `PrintReceiptRequest`:

```json
{
  "command": "BASE64_ENCODED_COMMAND",
  "hostnameOrIp": "192.168.19.11",
  "port": 9100
}
```

Campos:
- `command` *(string)*: comando ESC/POS em **Base64**
- `hostnameOrIp` *(string)*: IP/host da impressora
- `port` *(int)*: porta TCP (ex.: 9100)

---

## ✅ Respostas

### Sucesso (200 OK)
```json
{ "success": true, "message": "Impresso com sucesso" }
```

### Erro (500)
```json
{ "success": false, "message": "Falha na impressão" }
```

---

## 🧪 Teste rápido (cURL)

> Substitui o `BASE64_ENCODED_COMMAND` pelo teu comando real.

```bash
curl -k -X POST "https://localhost:5001/Print"   -H "Content-Type: application/json"   -d '{"command":"BASE64_ENCODED_COMMAND","hostnameOrIp":"192.168.19.11","port":9100}'
```

Se estiver em HTTP:
```bash
curl -X POST "http://localhost:5000/Print"   -H "Content-Type: application/json"   -d '{"command":"BASE64_ENCODED_COMMAND","hostnameOrIp":"192.168.19.11","port":9100}'
```

---

## 🛠️ Como gerar o comando ESC/POS (cliente)

A API espera **bytes ESC/POS** em Base64.  
Exemplos de origem desses bytes:

- Aplicação C#/POS que usa `ESCPOS_NET.Emitters` e depois faz `Convert.ToBase64String(bytes)`
- Serviço backend que monta o recibo e chama a API
- Frontend (menos comum) chamando backend que gera o comando

Exemplo (C#) de conversão para Base64:

```csharp
byte[] bytes = /* teu comando esc/pos */;
string base64 = Convert.ToBase64String(bytes);
```

---


## 🔌 Exemplo completo (Demo) — Gerar comando ESC/POS + chamar a API

Abaixo vai um exemplo **genérico** (sem dados reais de empresa/cliente) mostrando o fluxo típico:

1) O teu sistema (ex.: MVC/ERP) **gera os bytes ESC/POS** com `ESCPOS_NET` e devolve o comando em **Base64**  
2) O frontend (JS/jQuery) chama a tua **APIPrinter** enviando `{ command, hostnameOrIp, port }`

> ⚠️ Nota: este exemplo é apenas uma **demo** com texto simples. Ajusta rotas/URLs conforme o teu ambiente.

---

### 1) Backend (ex.: ASP.NET MVC) — gerar um comando simples e retornar Base64

```csharp
using ESCPOS_NET.Emitters;
using ESCPOS_NET.Utilities;
using Microsoft.AspNetCore.Mvc;
using System;

[HttpPost]
public ActionResult CreatePrint(int idVenda)
{
    // DEMO: em produção, aqui irias carregar a venda/itens da BD.
    var e = new EPSON();

    try
    {
        // Exemplo simples (sem dados reais)
        var commandBytes = ByteSplicer.Combine(
            e.CenterAlign(),
            e.SetStyles(PrintStyle.Bold),
            e.PrintLine("MINHA LOJA (DEMO)"),
            e.SetStyles(PrintStyle.None),
            e.PrintLine(""),
            e.LeftAlign(),
            e.PrintLine("Recibo: DEMO-0001"),
            e.PrintLine("Data: " + DateTime.Now.ToString("dd-MM-yyyy HH:mm")),
            e.PrintLine("-----------------------------------------------"),
            e.PrintLine("Item A                 1 x 100,00      100,00"),
            e.PrintLine("Item B                 2 x  50,00      100,00"),
            e.PrintLine("-----------------------------------------------"),
            e.RightAlign(),
            e.PrintLine("TOTAL: 200,00"),
            e.PrintLine(""),
            e.CenterAlign(),
            e.PrintLine("Obrigado!"),
            e.PartialCutAfterFeed(5)
        );

        string commandBase64 = Convert.ToBase64String(commandBytes);

        return Json(new { success = true, command = commandBase64 });
    }
    catch
    {
        return Json(new { success = false, message = "Falha ao criar comando" });
    }
}
```

---

### 2) Frontend (jQuery/AJAX) — obter Base64 do teu backend e enviar para a APIPrinter

> Importante: a tua APIPrinter espera **estes campos** no JSON:
> - `command`
> - `hostnameOrIp`
> - `port`

```javascript
function PrintReceipt(idVenda) {
  $.ajax({
    type: "POST",
    async: true,
    url: "/Home/CreatePrint",
    data: { idVenda: idVenda },
    success: function (response) {
      if (!response.success) {
        Swal.fire("Falha ao gerar comando!", "", "error");
        return;
      }

      const commandBase64 = response.command;

      $.ajax({
        type: "POST",
        url: "https://localhost:5001/Print", // URL da APIPrinter (ajusta)
        data: JSON.stringify({
          command: commandBase64,
          hostnameOrIp: "192.168.1.50", // IP/Host da impressora (demo)
          port: 9100                    // Porta da impressora (demo)
        }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (printResponse) {
          if (printResponse.success) {
            Swal.fire("Impresso com sucesso!", "", "success");
          } else {
            Swal.fire("Falha na impressão!", "", "error");
          }
        },
        error: function () {
          Swal.fire("Erro ao chamar a API de impressão!", "", "error");
        }
      });
    },
    error: function () {
      Swal.fire("Erro ao gerar comando!", "", "error");
    }
  });
}
```

---

### 3) Observações importantes

- Se o browser bloquear por CORS, confirma a policy no `Program.cs` (em produção, restringe origens).
- Se usares HTTPS local, pode ser necessário aceitar o certificado (ou testar via HTTP).
- O comando em Base64 deve conter **bytes ESC/POS válidos** (gerados pelo teu backend).

---

## 🧯 Troubleshooting

### Timeout / Não imprime
- Confirma IP e porta da impressora (ex.: 9100)
- Testa conectividade:
  - `ping 192.168.19.11`
  - Windows PowerShell: `Test-NetConnection 192.168.19.11 -Port 9100`
- Firewall pode bloquear a porta

### Imprime “lixo” / Acentos errados
- Ajusta **encoding/code page** no gerador do comando ESC/POS
- Muitos modelos exigem code pages específicas (CP850/CP860/CP857 etc.)

---

## 🗂️ Logging

Há um serviço `Global` que grava logs em:
- `wwwroot/Logs/logfile.txt`

Se `wwwroot` não existir (ou `_env.WebRootPath` vier vazio), cria a pasta `wwwroot` no projeto ou ajusta para `_env.ContentRootPath`.

---

## 🔐 Recomendações (Produção)

- Restringir CORS para as origens reais
- Proteger com API Key/JWT
- Não expor a API diretamente à internet pública (usar rede interna/VPN)
- Logs por Job + Retentativas (se necessário)

---

##🗺️ Roadmap (sugestões)

 - Swagger completo com exemplos

 - Endpoint para imprimir imagem/QRCode

 - Fila de impressão (background queue) para evitar bloquear requests

 - Templates de recibo (layout) por loja/empresa

 - Retentativas e logs por job
 
---

##🤝 Contribuição

1. Fork

2. Branch: feature/minha-melhoria

3. Pull request

## 📄 Licença

MIT [MIT license](LICENÇA)..
