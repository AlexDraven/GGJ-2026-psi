# GAMEPLAY

Documento de referencia del gameplay del juego. Está pensado para **desarrolladores** y **agentes de IA**: cualquier agente que extienda o modifique el juego debe usar este archivo como contexto para entender el diseño objetivo y la base de código existente.

---

## 1. Propósito del documento

- Describir el **gameplay objetivo**: interacción con NPCs/objetos, diálogos con opciones y consecuencias en psicodelia y felicidad, efectos visuales (cámara psicodélica, cara del personaje).
- Mapear **qué existe ya** en el código y **qué falta** por implementar.
- Ser lo bastante **específico** para que un agente de IA entienda el diseño y extienda el juego sin ambigüedades.

---

## 2. Gameplay en una página

### Loop principal

El jugador se mueve por el mundo, se acerca a **NPCs** u **objetos interactuables**, pulsa **Interact** y se abre un **diálogo**: texto tipo typewriter y, al final, **opciones de respuesta**. Según la opción elegida y la **configuración del NPC/objeto**, el juego actualiza dos estadísticas del personaje:

- **Nivel de psicodelia**
- **Nivel de felicidad**

### Psicodelia

Valor numérico (p. ej. 0–1 o 0–100) que controla la **intensidad de un efecto visual en pantalla completa** (shader/post-proceso en cámara). A mayor nivel, más “psicodélico” se ve **todo el juego**. Es un único efecto de cámara/post-proceso cuya intensidad depende del nivel de psicodelia.

### Felicidad

Valor numérico que se refleja en la **cara del personaje** mostrada en pantalla: UI fija en la parte **inferior central** (estilo Doom: retrato/cara que cambia según estado). No es un HUD genérico; es concretamente la **cara del protagonista**. La expresión o el frame de esa cara depende del nivel de felicidad (p. ej. rangos: muy bajo = triste, medio = neutro, alto = feliz).

---

## 3. Diagrama de flujo

```mermaid
flowchart LR
    A[Player se acerca] --> B[Interact]
    B --> C[DialogueManager muestra líneas y opciones]
    C --> D[Jugador elige opción]
    D --> E[Aplicar deltas según config NPC/objeto]
    E --> F[Actualizar psicodelia y felicidad]
    F --> G[Cerrar diálogo]
```

---

## 4. Sistemas detallados

### 4.1 Interacción: NPCs y objetos

- **Quién puede dar diálogo:** NPCs (ya soportados) y **objetos** (objetivo futuro: mismos datos de diálogo + opciones + configuración de consecuencias).
- **Datos por NPC/objeto:**
  - Texto del diálogo (líneas).
  - Opciones de respuesta (lista de strings).
  - **Configuración por opción:** Para cada opción (índice 0, 1, 2…), definir:
    - **Delta de psicodelia** (aumentar o disminuir; ej. +0.2, -0.1).
    - **Delta de felicidad** (aumentar o disminuir; ej. +10, -5).
  - Opcional: nombre del hablante (para NPCs; objetos pueden usar un nombre o “Objeto”).

**Base actual:** `Assets/scripts/NpcController.cs` tiene `speakerName`, `dialogueText`, `dialogueChoices` y un único `choiceIndexThatTriggersEffect` que hoy solo sube psicodelia para una opción. Falta: configuración por opción con deltas de psicodelia y felicidad (subir/bajar ambos).

### 4.2 Flujo de diálogo

- **Existente:** `Assets/scripts/DialogueManager.cs` — `StartDialogue(speaker, lines, choices, owner)`, typewriter, navegación de opciones (UI/joystick), `Advance()` al confirmar. Al elegir opción: si `dialogueOwner.ChoiceIndexThatTriggersEffect == choiceSelectedIndex` llama a `PsychedelicCameraEffect.Instance.AddIntensity(0.2f)` y guarda `GameController.Instance.LastDialogueResponse`.
- **Objetivo:** En lugar de un solo “índice que activa efecto”, cada opción debe tener **deltas configurables** (psicodelia y felicidad). Al confirmar opción, el juego aplica esos deltas a los niveles globales y la cámara/cara se actualizan según esos niveles.

### 4.3 Nivel de psicodelia y efecto de cámara

- **Existente:** `Assets/scripts/PsychedelicCameraEffect.cs` — Singleton en la cámara, `intensity` 0–1, `AddIntensity(amount)`, decay opcional, shader `Hidden/PsychedelicEffect` en `OnRenderImage`. El juego ya aplica un efecto visual a toda la pantalla según una intensidad.
- **Objetivo:** Tratar esa intensidad como el **nivel de psicodelia** del personaje (o derivarlo de un “nivel de psicodelia” en GameController/PlayerState que luego se mapee a intensidad de cámara). Las elecciones de diálogo suben o bajan ese nivel; la cámara siempre refleja el nivel actual.

### 4.4 Felicidad y cara del personaje (estilo Doom)

- **Objetivo:** Una UI fija en la parte **inferior central** de la pantalla que muestra la **cara del personaje** (sprite/animación). La expresión o el frame de esa cara depende del **nivel de felicidad** (p. ej. rangos: muy bajo = triste, medio = neutro, alto = feliz).
- **No existe aún:** No hay componente de “face UI” ni variable de felicidad en el código. Especificación: posición (abajo centro), que es la cara del protagonista, y que el valor de felicidad (a persistir en GameController o similar) determina qué se muestra.

---

## 5. Estado actual del código

| Concepto | Archivo / componente | Estado |
|----------|----------------------|--------|
| Diálogo con opciones | DialogueManager, DialogueUI, CreateDialogueUI | Hecho (falta vincular deltas por opción) |
| NPC que inicia diálogo | NpcController (trigger 2D, StartDialogue) | Hecho (falta config de deltas por opción) |
| Interact | PlayerController (Interact, radio, NPC más cercano) | Hecho |
| Efecto psicodélico cámara | PsychedelicCameraEffect | Hecho (falta que lea “nivel” global y permita bajar) |
| Respuesta última | GameController.LastDialogueResponse | Hecho |
| Nivel psicodelia (estado global) | — | Por implementar (o reutilizar intensity como nivel) |
| Nivel felicidad (estado global) | — | Por implementar |
| Cara personaje (Doom) | — | Por implementar |
| Objetos interactuables | — | Por implementar (mismo contrato que NPC: diálogo + opciones + deltas) |

---

## 6. Especificaciones para agentes IA

- **Ubicación de datos de diálogo:** NpcController en el inspector (y en el futuro, un componente equivalente en objetos). Extensión: por cada opción, dos valores numéricos (delta psicodelia, delta felicidad).
- **Dónde persistir niveles:** Un único lugar (p. ej. GameController o un PlayerState singleton) con `PsychedeliaLevel` y `HappinessLevel`, leídos por PsychedelicCameraEffect y por el componente de la cara.
- **Contrato de DialogueManager al confirmar opción:** Recibir el índice elegido y el `owner` (NpcController u objeto); el owner proporciona los deltas para ese índice; el juego aplica los deltas y actualiza UI/cámara.
- **Input:** Se usa Input System con action maps `"Player"` y `"UI"`; durante el diálogo el movimiento está deshabilitado y la navegación de opciones usa `"UI/Navigate"`.
