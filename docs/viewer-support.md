# Viewer: supported commands

`^XA` and `^XZ` delimit labels. Other commands that are not listed are reported in
`AnalyzeInfo.UnknownCommands` and ignored.

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
| `^BQ` | QR Code | model 2, error correction, mask, mixed mode data; model 1 is parsed but drawn as model 2 (no model 1 encoder); any other model falls back to 2 and a mask value outside 0 - 7 to 7 (the defaults) |
| `^BS` | UPC/EAN extension (2 and 5 digits) | |
| `^BU` | UPC-A | |
| `^BX` | Data Matrix | ECC 200, module size, rows and columns (when both are set); format, escape character and aspect ratio are parsed but not used for drawing |
| `^BY` | Barcode field defaults | module width, ratio, height |

Not supported: Code 49 (`^B4`), Planet (`^B5`), Plessey (`^BP`), POSTNET (`^BZ`), GS1 DataBar (`^BR`),
CODABLOCK (`^BB`), TLC39 (`^BT`), Micro PDF417 (`^BF`), Micro QR.

## Text

| Command | Description |
| --- | --- |
| `^A` | Scalable / bitmap font (`0`, `A` - `Z`, `1` - `9`), orientation, size |
| `^CF` | Change default font |
| `^CI` | Change international font / encoding |
| `^FB` | Field block: width, lines, line spacing, justification (L, C, R, J), hanging indent (applies to lines 2+; centered, right aligned and justified lines use the area from the indent to the block width) |
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
| `^GF` | Graphic field (format A: ASCII hex, compressed `:Z64:` / `:B64:` and ACS data; the format parameter defaults to A; format B (binary) is rejected with an entry in `AnalyzeInfo.Errors`) |
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
| `~DG` | Download graphic (GRF), stored as `NAME.GRF`; an omitted name is `UNKNOWN` |
| `~DY` | Download object, stored under name and extension (`~DYR:LOGO,P,P,...` as `LOGO.PNG`); PNG only, GRF objects are not decoded yet |
| `^DF` | Store format |
| `^XF` | Recall format |
| `^FN` | Field number |

## Behavior notes and limitations

- Stored formats (`^DF`/`^XF`) are keyed by storage device and name: the device defaults to `R:`, the name is
  case-insensitive and the extension is ignored. A later `^DF` (and `~DG`/`~DY`) with the same key replaces the
  earlier one. Recalling a format that was not stored is reported in `AnalyzeInfo.Errors`; the label keeps its own elements.
- `^FN` with `^FD` in a field that has a position (`^FO`/`^FT`) defines the field and provides the data for all
  fields with this number. Without a position (as in a `^XF` label) it only provides data. The optional
  quoted prompt (`^FN1"Name"`) is ignored. Data given with `^FN`/`^FD` in a `^DF` format is the default of all fields
  with that number: it prints when the recalling label supplies no data for the field number, and data of the
  recalling label overrides it. A field without any data is not printed.
- Reverse print (`^FR`, `^LR`) fields are drawn normally into a layer that is inverted once, so overlapping parts of
  one field (e.g. extra `^FB` lines that do not fit) do not cancel each other out.
- The escape character of a Data Matrix field (`^BX` parameter g, default `~`) is not treated as the start of a
  command inside the `^FD`/`^FV` of that field.
- Carriage returns and line feeds are ignored everywhere in the ZPL stream, also inside `^FD`. Use `\&` in
  `^FB` fields (or `^FH` escapes) for line breaks.
- `^FH` indicators that are invalid (`^`, `~`, control and non-ASCII characters) fall back to `_`. A space is a
  valid indicator.
- `^CF` keeps the last height and width if a value is omitted. `^FW` only applies to `^A` fonts without an
  explicit orientation, also when it follows `^A` in the same field.
- `^FT` with an omitted y coordinate cannot continue at the end of the previous text field (the drawers only support
  the complete default position). The y coordinate of the last positioned field is used and the x coordinate is kept.
  An omitted x coordinate with explicit y (`^FT,y`) is supported for text fields and field blocks; other elements
  continue at the complete default position.
- A right justification of `^FO`/`^FT` (third parameter) makes the position the right edge of graphics, images and
  field blocks. `^FT` elements above the label are clipped, not moved.
- One `ZplElementDrawer` can be used by several threads; `FontManager` is `IDisposable` (dispose it when the
  `DrawerOptions` are no longer used).
- Out of range values are clamped (`^BY` module width 1 - 10, ratio 2.0 - 3.0; `^FB` width 0 - 32000, lines
  1 - 9999, line spacing -9999 - 9999, indent 0 - 9999); a non-finite `^BY` ratio is ignored.

## Not supported

- Font download commands (`~DB`, `~DS`, `~DT`, `~DU`, `^DY` fonts) - map fonts with `FontManager`, see [Viewer](viewer.md)
- Printer configuration and media commands (`^PW`, `^LL`, `^MM`, `^PR`, ...) are ignored; pass the label size to `Draw`
- Serialization (`^SN`, `^SF`) and RFID commands
