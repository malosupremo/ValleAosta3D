# ValleAosta3D

Progetto personale per la generazione e stampa 3D di un modello fisico della Valle d'Aosta utilizzando dati altimetrici reali (DEM).

---

# Obiettivo

Realizzare un plastico della Valle d'Aosta stampabile con una stampante 3D desktop, partendo da un Digital Elevation Model (DEM) reale.

Pipeline prevista:

```text
DEM
 ↓
Heightmap
 ↓
Mesh STL
 ↓
Suddivisione in tessere
 ↓
Slicer
 ↓
GCODE
 ↓
Stampa 3D
```

---

# Hardware

## Stampante

```text
Entina Tina2
```

Caratteristiche considerate:

```text
Volume utile:
100 x 105 x 100 mm circa

Ugello:
0.4 mm

Layer tipico:
0.15 mm

Layer fine:
0.06 mm
```

---

# Modello

## Scala orizzontale

```text
1 : 400.000
```

Equivalenze:

```text
1 mm = 400 m
1 cm = 4 km
```

---

## Esagerazione verticale

```text
4x
```

Motivazione:

A scala reale, il Monte Bianco sarebbe alto circa:

```text
4810 m / 400000
=
12 mm
```

Con un'esagerazione verticale di 4x:

```text
12 mm x 4
=
48 mm
```

Il modello risulta molto più leggibile e spettacolare.

---

# Dimensioni del plastico

## Configurazione attuale

```text
Tiles X       = 3
Tiles Y       = 2

Tile Size     = 90 mm
```

Dimensioni finali:

```text
270 mm x 180 mm
```

ovvero:

```text
27 cm x 18 cm
```

---

## Copertura reale

Alla scala configurata:

```text
108 km x 72 km
```

---

## Suddivisione

```text
+-----+-----+-----+
|  A  |  B  |  C  |
+-----+-----+-----+
|  D  |  E  |  F  |
+-----+-----+-----+
```

Numero totale tessere:

```text
6
```

Dimensione di ogni tessera:

```text
90 mm x 90 mm
```

Copertura reale di una tessera:

```text
36 km x 36 km
```

---

# Area geografica

## Centro

```text
Latitudine  = 45.750000
Longitudine = 7.430000
```

Il centro è stato scelto per mantenere la Valle d'Aosta approssimativamente al centro del modello.

---

## Bounding Box calcolato

Derivato automaticamente da:

- centro geografico
- scala
- dimensioni modello

Coordinate attuali:

```text
South = 45.426608
West  = 6.734823

North = 46.073392
East  = 8.125177
```

---

# Risoluzione orizzontale

Configurazione attuale:

```text
0.1 mm
```

Interpretazione:

```text
0.1 mm sul modello
=
40 m reali
```

---

## Campionamento teorico

Dimensioni del modello:

```text
270 mm x 180 mm
```

Con risoluzione:

```text
0.1 mm
```

si ottiene:

```text
Width Samples  = 2700
Height Samples = 1800
```

Totale:

```text
4.860.000 campioni
```

---

# Dati altimetrici

## Sorgente selezionata

OpenTopography

Dataset:

```text
COP30
Copernicus Global DSM 30m
```

Motivazioni:

- gratuito
- disponibile via API
- output GeoTIFF
- qualità sufficiente per il progetto
- copertura globale

---

# Test effettuati

## Download ridotto

Bounding box ridotto.

Risultato:

```text
720 x 360 pixel
~1 MB
```

Verificato:

- download corretto
- GeoTIFF valido
- file prodotto da GDAL

---

## Download completo

Bounding box dell'intero plastico:

```text
South = 45.426608
West  = 6.734823

North = 46.073392
East  = 8.125177
```

Risultato:

```text
5005 x 2328 pixel
48.984.705 bytes
```

Osservazioni:

```text
5005 x 2328
=
11.651.640 campioni
```

Il dataset scaricato contiene più dettaglio di quanto il modello possa sfruttare.

Questo è ideale perché permette successivi processi di riduzione controllata.

---

# Cache

Tutti i dati scaricati devono essere salvati localmente.

Struttura prevista:

```text
Data
|
+-- Cache
|    |
|    +-- cop30-valle-aosta.tif
|
+-- Output
```

Il progetto deve poter funzionare anche offline dopo il primo download.

---

# Stato del progetto

## Completato

- [x] Repository GitHub creato
- [x] README iniziale
- [x] .editorconfig
- [x] Configurazione tramite appsettings.json
- [x] Modello tipizzato delle opzioni
- [x] Struttura cartelle
- [x] Calcolo dimensioni modello
- [x] Calcolo bounding box
- [x] Calcolo campionamento teorico
- [x] Ottenimento API Key OpenTopography
- [x] Download DEM completo
- [x] Validazione GeoTIFF

---

## Prossima milestone

Analisi del GeoTIFF.

Installare:

```text
BitMiracle.LibTiff.NET
```

e creare:

```text
GeoTiffInspector
```

con lettura di:

- Width
- Height
- BitsPerSample
- SamplesPerPixel
- Compression
- SampleFormat
- Min Elevation
- Max Elevation
- NoData Value

---

## Milestone successive

### Preview

```text
GeoTIFF
 ↓
PNG grayscale
```

Output:

```text
Data/Output/preview.png
```

---

### Terrain Model

```text
GeoTIFF
 ↓
float[,]
 ↓
HeightMap
```

---

### STL

```text
HeightMap
 ↓
Mesh triangolare
 ↓
STL Binary
```

---

### Tile Split

Generazione automatica di:

```text
tile_00.stl
tile_01.stl
tile_02.stl

tile_10.stl
tile_11.stl
tile_12.stl
```

---

### Stampa

```text
STL
 ↓
Slicer
 ↓
GCODE
 ↓
Entina Tina2
```

---

# Considerazioni

Il download completo del DEM (~49 MB, 5005x2328 pixel) rappresenta il primo vero dato geografico del progetto.

La risoluzione ottenuta è superiore alla risoluzione teoricamente necessaria per il modello finale, rendendo il dataset una base eccellente per la generazione della heightmap e delle successive mesh STL.