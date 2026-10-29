# Viewer: supported commands

Commands that are not listed are reported in `AnalyzeInfo.UnknownCommands` and ignored.

## Barcodes

| Command | Symbology | Notes |
| --- | --- | --- |
| `^B0`, `^BO` | Aztec | |
| `^B1` | Code 11 | one or two check digits |
| `^B2` | Interleaved 2 of 5 | optional mod 10 check digit |
| `^B3` | Code 39 | optional mod 43 check digit |
| `^B7` | PDF417 | rows, columns, security level, truncation |
| `^B8` | EAN-8 | |
| `^B9` | UPC-E | |
| `^BA` | Code 93 | check digits |
| `^BC` | Code 128 | modes N, U, A, D (GS1 / UCC) |
| `^BD` | MaxiCode | modes 2 - 6 |
| `^BE` | EAN-13 | |
| `^BI` | Industrial 2 of 5 | |
| `^BK` | ANSI Codabar | start and stop characters |
| `^BL` | LOGMARS | |
| `^BM` | MSI | check digit modes |
| `^BQ` | QR Code | model 2, error correction, mask, mixed mode data |
| `^BS` | UPC/EAN extension (2 and 5 digits) | |
| `^BU` | UPC-A | |
| `^BX` | Data Matrix | ECC 200, rows/columns, format, escape sequence, aspect ratio |
| `^BY` | Barcode field defaults | module width, ratio, height |

Not supported: Code 49 (`^B4`), Planet (`^B5`), Plessey (`^BP`), POSTNET (`^BZ`), GS1 DataBar (`^BR`),
CODABLOCK (`^BB`), TLC39 (`^BT`), Micro PDF417 (`^BF`), Micro QR.

## Text

| Command | Description |
| --- | --- |
| `^A` | Scalable / bitmap font (`0`, `A` - `Z`, `1` - `9`), orientation, size |
| `^CF` | Change default font |
| `^CI` | Change international font / encoding |
| `^FB` | Field block: width, lines, line spacing, justification (L, C, R, J), hanging indent |
| `^FD`, `^FV` | Field data, field variable |
| `^FH` | Hexadecimal indicator |
| `^FR` | Field reverse print |
| `^FW` | Default field orientation and justification |

## Graphics

| Command | Description |
| --- | --- |
| `^GB` | Graphic box, including rounding and line color |
| `^GC` | Graphic circle |
| `^GD` | Graphic diagonal line |
| `^GE` | Graphic ellipse |
| `^GF` | Graphic field (ASCII hex, compressed `:Z64:` / `:B64:` and ACS data) |
| `^GS` | Graphic symbol (®, ©, ™, UL, CSA) |
| `^IM` | Image move (recall `~DY` object) |
| `^XG` | Recall graphic, with magnification |

## Positioning and label

| Command | Description |
| --- | --- |
| `^FO` | Field origin |
| `^FT` | Field typeset (baseline positioning) |
| `^FS` | Field separator - fields are finalized here |
| `^LH` | Label home |
| `^LR` | Label reverse print |
| `^CC`, `^CT` | Change caret / tilde character |
| `^FX` | Comment |

## Storage and formats

| Command | Description |
| --- | --- |
| `~DG` | Download graphic (GRF) |
| `~DY` | Download object (PNG; GRF objects are not decoded yet) |
| `^DF` | Store format |
| `^XF` | Recall format |
| `^FN` | Field number |

## Not supported

- Font download commands (`~DB`, `~DS`, `~DT`, `~DU`, `^DY` fonts) - map fonts with `FontManager`, see [Viewer](viewer.md)
- Printer configuration and media commands (`^PW`, `^LL`, `^MM`, `^PR`, ...) are ignored; pass the label size to `Draw`
- Serialization (`^SN`, `^SF`) and RFID commands
