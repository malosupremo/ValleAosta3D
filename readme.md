# ValleAosta3D

Progetto personale per la generazione e stampa 3D di un modello fisico della Valle d'Aosta utilizzando dati DEM (Digital Elevation Model) ad alta risoluzione.

## Obiettivo

Realizzare un plastico della Valle d'Aosta stampabile con una stampante 3D desktop (Entina Tina2), suddiviso in più tessere assemblabili.

L'obiettivo finale è:

- scaricare dati altimetrici reali;
- generare una heightmap;
- creare automaticamente mesh STL;
- suddividere il modello in tessere stampabili;
- esportare gli STL da utilizzare nello slicer per la generazione del GCODE.

---

# Specifiche del progetto

## Scala

Scala orizzontale:

```text
1 : 400.000
```

Significa:

```text
1 mm = 400 m
1 cm = 4 km
```

## Dimensioni finali

Target:

```text
27 cm x 18 cm
```

Equivalenti a:

```text
108 km x 72 km
```

nel mondo reale.

## Esagerazione verticale

Per migliorare la leggibilità del terreno:

```text
Vertical Exaggeration = 4x
```

Esempio:

```text
Monte Bianco = 4810 m
4810 / 400000 = 12 mm

12 mm x 4 = 48 mm
```

Altezza risultante:

```text
circa 4.8 cm
```

---

# Stampante

Modello:

```text
Entina Tina2
```

Caratteristiche considerate:

```text
Volume utile:
100 x 105 x 100 mm circa

Dimensione tassello:
90 x 90 mm

Layer tipico:
0.15 mm

Layer fine:
0.06 mm

Ugello:
0.4 mm
```

La risoluzione reale del modello sarà limitata principalmente dal nozzle da 0.4 mm.

---

# Suddivisione del modello

Ogni tessera:

```text
90 x 90 mm
```

Alla scala scelta:

```text
90 mm = 36 km
```

Configurazione:

```text
3 colonne x 2 righe
```

Schema:

```text
+-----+-----+-----+
|  A  |  B  |  C  |
+-----+-----+-----+
|  D  |  E  |  F  |
+-----+-----+-----+
```

Dimensione assemblata:

```text
27 cm x 18 cm
```

Totale tessere:

```text
6
```

---

# Area geografica

## Regione

Valle d'Aosta

Coordinate centrali approssimative:

```text
Latitudine : 45.750
Longitudine: 7.430
```

## Bounding Box del plastico

Calcolato per coprire:

```text
108 km x 72 km
```

con la Valle d'Aosta approssimativamente centrata.

Coordinate utilizzate:

```text
South = 45.426
West  = 6.734

North = 46.074
East  = 8.126
```

Formato API:

```text
SW = 45.426,6.734
NE = 46.074,8.126
```

---

# Sorgente dati altimetrici

## TessaDEM

Sito:

https://tessadem.com/

Documentazione API:

https://tessadem.com/elevation-api/

Caratteristiche principali:

- DEM globale
- risoluzione spaziale nominale 30 m
- output GeoTIFF
- API HTTP
- modalità:
  - points
  - path
  - area

Per questo progetto verrà utilizzata:

```text
mode=area
```

e:

```text
format=geotiff
```

---

# Chiamata API prevista

Template:

```text
https://tessadem.com/api/elevation
?key=API_KEY
&mode=area
&rows=128
&columns=128
&format=geotiff
&locations=45.426,6.734|46.074,8.126
```

---

# Architettura prevista

## Fase iniziale

Un singolo progetto console:

```text
ValleAosta3D.Console
```

Obiettivo:

```text
Scaricare il primo GeoTIFF
```

Nessuna complessità aggiuntiva.

---

# Pipeline prevista

```text
TessaDEM API
        |
        V
    GeoTIFF
        |
        V
   float[,]
        |
        V
 Heightmap PNG
        |
        V
   Mesh STL
        |
        V
 Suddivisione 3x2
        |
        V
    STL finali
        |
        V
      GCODE
```

---

# Struttura repository (provvisoria)

```text
ValleAosta3D
|
+-- src
|    |
|    +-- ValleAosta3D.Console
|
+-- docs
|
+-- samples
|
+-- README.md
```

---

# Milestone

## Milestone 1

Scaricare un GeoTIFF valido dal servizio TessaDEM.

Output atteso:

```text
Output/valle-aosta-128.tif
```

---

## Milestone 2

Leggere il GeoTIFF.

Obiettivo:

```csharp
float[,] elevations;
```

Verificare:

- quota minima
- quota massima
- dimensioni della griglia

---

## Milestone 3

Generare una PNG grayscale.

Verifica visiva del DEM.

Output:

```text
valle-aosta.png
```

---

## Milestone 4

Generare uno STL unico.

Output:

```text
valle-aosta.stl
```

---

## Milestone 5

Suddividere automaticamente il modello.

Output:

```text
tile_00.stl
tile_01.stl
tile_02.stl

tile_10.stl
tile_11.stl
tile_12.stl
```

---

## Milestone 6

Aggiungere:

- base del modello
- incastri
- magneti
- etichette
- ottimizzazione mesh

---

# Note tecniche

## Risoluzione DEM

DEM TessaDEM:

```text
30 m
```

Alla scala:

```text
30 m / 400000
=
0.075 mm
```

sul modello.

Questo significa che il dataset sorgente possiede una risoluzione molto più elevata di quella realmente stampabile.

Probabilmente sarà necessario un downsampling prima della generazione della mesh.

## Strategia consigliata

NON generare prima gli STL.

Procedere invece in questo ordine:

```text
DEM
 ->
PNG Heightmap
 ->
Verifica visiva
 ->
STL
```

Le immagini sono molto più semplici da validare e debuggare rispetto a una mesh triangolare.