# Generador de Terreno(Caminos)

Este documento proporciona una descripción detallada de las variables y configuraciones utilizadas en el **Generador de Terreno (Caminos)**. Las variables permiten ajustar cómo se crea y se comporta la mazmorras en el entorno del juego.

## Configuración Básica

### `Seed`
- **Tipo:** `int`
- **Descripción:**  
  Establece la semilla del generador aleatorio. Cambiar este valor generará un conjunto diferente de mazmorras, asegurando que las mazmorras sean únicas en cada ejecución si se usa un valor diferente.

### `ChunkSize`
- **Tipo:** `int`
- **Descripción:**  
  Define el tamaño de cada "trozo" o "chunk" de la mazmorras que se genera. Esto determina la cantidad de celdas que componen cada sección de la mazmorras.

### `ChunkNumSize`
- **Tipo:** `int`
- **Descripción:**  
  Especifica el número de chunks que se generarán en el entorno de la mazmorras. Esto define la extensión total de la mazmorras en términos de chunks generados.

### `BifurcationIncrease`
- **Tipo:** `float` (Rango: `0.0 - 1.0`)
- **Descripción:**  
  Ajusta el nivel de aumento de bifurcaciones en el camino generado. Un valor mayor hará que los caminos generados sean más ramificados, mientras que un valor bajo limitará las bifurcaciones y los caminos serán más rectos.

## Configuración Adicional

### `centerFactor`
- **Tipo:** `float` (Rango: `0.0 - 1.0`)
- **Descripción:**  
  Controla el movimiento desde el centro de la mazmorras. Un valor de `0.0` hace que el camino se centre completamente en la mazmorras, mientras que un valor de `1.0` puede hacer que el camino se aleje más del centro.

### `pathIrregularity`
- **Tipo:** `float` (Rango: `0.0 - 1.0`)
- **Descripción:**  
  Establece el nivel de irregularidad en el camino generado. Un valor de `0.0` genera caminos muy rectos y directos, mientras que un valor de `1.0` hace que el camino sea más sinuoso e impredecible.

## Prefabs

### `gFloor`
- **Tipo:** `GameObject`
- **Descripción:**  
  Prefabricado que representa el suelo de la mazmorras. Este objeto se utilizará para generar las superficies sobre las que el jugador puede caminar.

### `gWall`
- **Tipo:** `GameObject`
- **Descripción:**  
  Prefabricado que representa las paredes de la mazmorras. Se utiliza para construir las barreras físicas de la mazmorras.

## Flujo de Trabajo
### 1. Inicialización
- La generación comienza con la configuración de la semilla aleatoria y la posición de inicio del primer chunk.

### 2. Generación de Caminos
- Cada chunk genera un camino dentro de él, usando A* para calcular rutas entre puntos válidos en el borde.
- Se ajustan las características del camino según la irregularidad y el centro, haciendo que el camino sea más impredecible y adaptado al entorno.

### 3. Visualización
- Se crean los prefabricados de suelo y pared para representar visualmente los caminos y las paredes en el mundo 3D.

### 4. Conexión entre Chunks
- Los chunks están interconectados, y la posición del siguiente chunk se calcula según el punto final del chunk actual, asegurando que el camino fluya de uno a otro sin interrupciones.

