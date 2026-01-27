# ESC/POS Printing API — Documentation

Purpose
- Document the endpoints, request/response shapes, examples and ESC/POS payload guidance for the ESC/POS Printing API in this repository.

Quick summary
- Target platform: .NET 8
- Typical use: send ESC/POS byte sequences (raw) or high-level print jobs to receipt printers (serial / TCP / USB).
- Authentication: API Key or Bearer token (example sections use `Authorization: Bearer <token>`).

Base URL
- Example: `https://{host}/api/`

Common HTTP headers
- `Authorization: Bearer <token>` (if enabled)
- `Content-Type: application/json`

Endpoints

1) POST /api/print
- Purpose: submit a print job (raw ESC/POS bytes or structured job).
- Request body (JSON) — minimal common model:
  - `printer`: string — logical printer name or connection descriptor
  - `connection`: object — connection details (type: `tcp` | `serial` | `usb`)
    - tcp: `{ "type": "tcp", "host": "192.168.0.100", "port": 9100 }`
    - serial: `{ "type": "serial", "portName": "COM3", "baudRate": 19200 }`
  - `payloadType`: `raw` | `commands`
  - `rawBase64`: string — base64-encoded raw ESC/POS bytes (used when `payloadType` == `raw`)
  - `commands`: array — high-level commands (optional abstraction)
  - `metadata`: object — optional (orderId, user, copies, etc.)
- Response (201 or 202):
  - `{ "jobId": "string", "status": "queued" }`
- Errors:
  - 400 Bad Request — validation
  - 401 Unauthorized
  - 404 Printer not found
  - 500 Internal error (printer offline)

2) GET /api/printers
- Purpose: list configured or discovered printers
- Response:
  - `[{ "name": "POS-Printer-1", "type": "tcp", "address": "192.168.0.100:9100", "status": "online" }]`

3) GET /api/jobs/{jobId}
- Purpose: get status for a print job
- Response:
  - `{ "jobId": "string", "status": "queued|printing|done|failed", "createdAt": "...", "error": null }`

4) GET /api/printer/{printerName}/status
- Purpose: health/status for a specific printer
- Response:
  - `{ "printer": "name", "status": "online|offline", "lastSeen": "..." }`

Request / Payload patterns

- Raw base64 example (JSON):
  - `{ "printer": "POS-01", "connection": { "type": "tcp", "host": "192.168.0.100", "port": 9100 }, "payloadType": "raw", "rawBase64": "<base64-of-bytes>" }`

- High-level `commands` example:
  - `{ "printer":"POS-01", "payloadType":"commands", "commands":[ {"type":"text","value":"Hello World"}, {"type":"feed","lines":2}, {"type":"cut","mode":"partial"} ] }`

ESC/POS basics (common byte sequences)
- Initialize: `0x1B 0x40`  (ESC @)
- New line / feed: `0x0A`  (LF) or `ESC d n` (`1B 64 n`)
- Bold ON / OFF: `1B 45 01` / `1B 45 00`
- Align center/left/right: `1B 61 01` (center), `1B 61 00` (left), `1B 61 02` (right)
- Cut paper: `1D 56 00` (full) or `1D 56 01` (partial)
- QR code: sequence varies by printer model — consult manufacturer
- Important: use raw bytes for control codes; prefer base64 in JSON to avoid encoding issues.

Example: build a simple ESC/POS raw payload (hex)
- Initialize + text + feed + cut:
  - Hex: `1B 40` + `48 65 6C 6C 6F 20 57 6F 72 6C 64` (ASCII "Hello World") + `0A` + `1D 56 00`
- Base64 for that byte array can be placed into `rawBase64`.

Examples

- CURL (raw base64)

- C# (.NET 8) — build ESC/POS bytes and call API

- Send bytes directly to a TCP printer (if you need client side)

Testing & troubleshooting
- Verify network reachability (ping/telnet host:9100).
- Try a small known-good payload (init + "test\n" + cut).
- If printing fails: examine printer logs (if available), check serial port permissions, confirm correct encoding and correct cut command for the model.
- For USB printers on Windows, consider Win32 Raw printing (Spooler) or vendor SDK.

Security & safety
- Sanitize templates / dynamic data before building raw bytes.
- Limit allowed printers and validate `connection` parameters on server side.
- Use TLS when sending raw print jobs over network.

Manufacturer differences
- ESC/POS is a de-facto standard; several commands vary. For advanced features (QR, raster images, barcode), check the specific printer manual.

Want more precise docs?
- I couldn't locate source code in the workspace to extract actual DTOs/controllers. If you want a tailored README matching your repository:
  - Provide the controller or DTO files (or open the active file), or
  - Give the project path to the API controllers (e.g., `src/Api/Controllers/PrintController.cs`).
I can then generate endpoint signatures, exact request models and example responses matching your code.