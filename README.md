# TraductorPersonal

**AutomaticoSubAnimeEs** es una herramienta que automatiza el proceso de traducir archivos de subtítulos en formato `.ass` de inglés a español utilizando un modelo de inteligencia artificial local. El modelo usado es proporcionado por [Helsinki-NLP](https://huggingface.co/Helsinki-NLP/opus-mt-en-es/tree/main).

## Características

- Traducción rápida y precisa de archivos `.ass`.
- Funcionamiento local sin necesidad de conexión constante a internet.
- Integración con modelos de Hugging Face.

## Requisitos previos

Antes de comenzar, asegúrate de tener instalados los siguientes componentes:

- Python 3.8 o superior
- `pip` (Administrador de paquetes de Python)
- Tener un modelo AI enfocado a traducir texto ingles a español, se ua Helsinki-NLP pero se puede usar otros modelos AI para la traducciones

### Librerías requeridas

Ejecuta el siguiente comando para instalar las dependencias:

```bash
pip install transformers torch
pip install sentencepiece
pip install sacremoses 
```

## Instalación

1. Clona el repositorio en tu máquina local:

2. Descarga el modelo de Hugging Face:

Sigue las instrucciones [aquí](https://huggingface.co/Helsinki-NLP/opus-mt-en-es/tree/main).

3. Asegúrate de que los archivos `.ass` que deseas traducir estén en la carpeta adecuada.
# TraductorPersonal

**TraductorPersonal** es una herramienta que automatiza el proceso de traducir archivos de subtítulos en formato `.ass` de inglés a español utilizando un modelo de inteligencia artificial local. El modelo usado es proporcionado por [Helsinki-NLP](https://huggingface.co/Helsinki-NLP/opus-mt-en-es/tree/main).

## Características

- Traducción rápida y precisa de archivos `.ass`.
- Funcionamiento local sin necesidad de conexión constante a internet.
- Integración con modelos de Hugging Face.

## Requisitos previos

Antes de comenzar, asegúrate de tener instalados los siguientes componentes:

- Python 3.8 o superior
- `pip` (Administrador de paquetes de Python)
- Una cuenta en [Hugging Face](https://huggingface.co/) para descargar el modelo si es necesario

### Librerías requeridas

Ejecuta el siguiente comando para instalar las dependencias:

```bash
pip install transformers ass
```

## Instalación

1. Clona el repositorio en tu máquina local:

```bash
git clone https://github.com/tu_usuario/TraductorPersonal.git
cd TraductorPersonal
```

2. Descarga el modelo de Hugging Face:

El script descargará automáticamente el modelo de Hugging Face, pero puedes hacerlo manualmente si lo prefieres. Sigue las instrucciones [aquí](https://huggingface.co/Helsinki-NLP/opus-mt-en-es/tree/main).

3. Edita las rutas en el proyecto:

   - **Abrir y editar el archivo `ModeloAI/ModeloRapido.py`**:
     - Ubicado en la carpeta `ModeloAI`.
     - Modifica la línea:
       ```python
       model_path = "Ruta"
       ```
       Reemplaza esta ruta con la ubicación exacta de tu modelo descargado de Hugging Face.

   - **Abrir y editar el archivo `Form1.cs`**:
     - Localiza la función `TranslateTextAsync` y actualiza las siguientes líneas:
       - Ruta del script Python (`ModeloRapido.py`):
         ```csharp
         string pythonScript = @"C:\Ruta\A\Tu\ModeloRapido.py";
         ```
       - Ruta del intérprete Python (`python.exe`):
         ```csharp
         string pythonInterpreter = @"C:\Ruta\A\Tu\Python\python.exe";
         ```
         4. Ejecutar el programa y empezar la traduccion automatica.
