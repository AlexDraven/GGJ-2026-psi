# Resolver "Can't Generate Mesh, No Font Asset has been assigned"

## Comprobación en este proyecto

- **Escena SampleScene:** No hay componentes TextMeshPro en la escena; el error puede venir de un objeto con TMP sin Font Asset (p. ej. menú de pausa con UI) o del Editor.
- **TMP Essential Resources:** No estaban importados en `Assets/` (no existe `Assets/TextMesh Pro/Resources/`). Sin ellos no hay Font Asset por defecto.

## Pasos para corregir

1. **Importar TMP Essential Resources**  
   - En Unity: **Window > TextMeshPro > Import TMP Essential Resources**  
   - O: **Tools > Fix TMP Font Asset (Import Essential Resources)**  
   - Confirma la importación del paquete (incluye LiberationSans SDF y TMP Settings).

2. **Definir Font Asset por defecto**  
   - **Edit > Project Settings > TextMesh Pro > Settings**  
   - En **Default Font Asset** asigna uno (p. ej. **LiberationSans SDF**).  
   - Así los nuevos textos TMP tendrán Font Asset asignado.

3. **Si ya tienes textos TMP en la escena**  
   - En la **Hierarchy**, selecciona cada objeto con **Text - TextMeshPro** o **Button - TextMeshPro**.  
   - En el **Inspector**, en el componente TMP, asigna el campo **Font Asset**.

4. **Menú de pausa**  
   - Si `pauseMenuObject` del Game Controller es un Canvas con textos TMP, abre ese prefab/objeto y asigna **Font Asset** a cada componente TextMeshPro.

## Atajos en el proyecto

- **Tools > Fix TMP Font Asset (Import Essential Resources):** abre la importación de TMP Essential Resources y muestra un recordatorio de los pasos.
- **Tools > Fix TMP Font Asset (Ver instrucciones):** muestra un diálogo con instrucciones según si TMP Settings está importado o no.
