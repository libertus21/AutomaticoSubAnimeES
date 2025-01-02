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

El script descargará automáticamente el modelo de Hugging Face, pero puedes hacerlo manualmente si lo prefieres. Sigue las instrucciones [aquí](https://huggingface.co/Helsinki-NLP/opus-mt-en-es/tree/main).

3. Asegúrate de que los archivos `.ass` que deseas traducir estén en la carpeta adecuada.

## Uso

1. Coloca el archivo `.ass` que deseas traducir en el directorio raíz del proyecto.

2. Ejecuta el script principal:

```bash
python traductor.py --archivo "ruta/del/archivo.ass"
```

### Opciones disponibles

- `--archivo`: Ruta del archivo `.ass` a traducir (obligatorio).
- `--salida`: Ruta del archivo de salida traducido (opcional).

Por ejemplo:

```bash
python traductor.py --archivo "subtitulos.ass" --salida "subtitulos_traducidos.ass"
```

## Ejemplo

Si tienes un archivo llamado `subtitulos.ass`, simplemente ejecuta:

```bash
python traductor.py --archivo "subtitulos.ass"
```

Esto generará un nuevo archivo llamado `subtitulos_traducidos.ass` en el mismo directorio.

## Contribuir

¡Las contribuciones son bienvenidas! Si deseas agregar nuevas características o corregir errores, sigue estos pasos:

1. Haz un fork del repositorio.
2. Crea una rama para tu nueva funcionalidad:

```bash
git checkout -b nueva-funcionalidad
```

3. Realiza los cambios necesarios y haz commit:

```bash
git commit -m "Descripción de los cambios"
```

4. Envía tus cambios con un pull request.

## Licencia

Este proyecto está bajo la Licencia MIT. Consulta el archivo `LICENSE` para más información.

---

Si tienes dudas o sugerencias, no dudes en abrir un issue en este repositorio.
