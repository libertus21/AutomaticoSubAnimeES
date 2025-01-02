# TraductorPersonal

**AutomaticoSubAnimeEs** es una herramienta avanzada que automatiza la traducción de archivos de subtítulos en formato `.ass` del inglés al español mediante el uso de un modelo de inteligencia artificial local. Este proyecto aprovecha el modelo de traducción de [Helsinki-NLP](https://huggingface.co/Helsinki-NLP/opus-mt-en-es/tree/main), aunque permite la flexibilidad de integrar otros modelos adaptados a tus necesidades.

## Características principales

- Traducción rápida y precisa de subtítulos en formato `.ass`.
- Funcionamiento completamente local, eliminando la necesidad de conexión constante a internet.
- Soporte para integrar otros modelos de Hugging Face o personalizados.

## Requisitos previos

Antes de comenzar, asegúrate de cumplir con los siguientes requisitos:

- **Python**: Versión 3.8 o superior.
- **pip**: Administrador de paquetes para Python.
- **Modelo AI**: Se recomienda usar el modelo de Helsinki-NLP para traducción de inglés a español, pero puedes emplear cualquier otro modelo compatible.

### Instalación de dependencias

Para instalar las librerías requeridas, ejecuta el siguiente comando:

```bash
pip install transformers torch sentencepiece sacremoses
```

## Instalación del proyecto

1. **Clonar el repositorio**

   Descarga el código fuente en tu máquina local:

   ```bash
   git clone https://github.com/libertus21/AutomaticoSubAnimeES.git
   cd TraductorPersonal
   ```

2. **Descargar el modelo AI**

   El script puede descargar automáticamente el modelo de Hugging Face. Si prefieres hacerlo manualmente, visita [Helsinki-NLP/opus-mt-en-es](https://huggingface.co/Helsinki-NLP/opus-mt-en-es/tree/main) y sigue las instrucciones de descarga.

3. **Configurar rutas en el proyecto**

   - **Archivo `ModeloAI/ModeloRapido.py`**:
     Edita la línea donde se define la ruta del modelo:
     ```python
     model_path = "Ruta"
     ```
     Reemplaza `"Ruta"` con la ubicación exacta donde se encuentra el modelo descargado.

   - **Archivo `Form1.cs`**:
     Localiza y actualiza las siguientes líneas en la función `TranslateTextAsync`:
     - **Ruta del script Python**:
       ```csharp
       string pythonScript = @"C:\Ruta\A\Tu\ModeloRapido.py";
       ```
     - **Ruta del intérprete de Python**:
       ```csharp
       string pythonInterpreter = @"C:\Ruta\A\Tu\Python\python.exe";
       ```

4. **Ejecutar el programa**

   Una vez configuradas las rutas, ejecuta el programa para comenzar la traducción automatizada.
