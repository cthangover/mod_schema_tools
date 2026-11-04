# Schema Tools — el Editor de datos y la paleta de comandos

Schema Tools son dos ventanas para autores, construidas por completo a partir de las descripciones que los mods incluyen como esquemas (ver el mod Schemas):

- el **Editor de datos** edita con formularios entidades de cualquier tipo descrito: atributos, clases, objetos, personajes, acciones de combate, tiendas, recetas;
- la **paleta de comandos** escribe una línea de escenario desde un formulario, la comprueba y la coloca en el editor de escenarios.

Ninguna de las dos ventanas conoce de antemano ningún tipo ni acción: en ellas aparece lo que describan los mods cargados. Las dos se abren desde la caja de herramientas de la barra del juego, con el juego en marcha.

## El Editor de datos

Elige un mod arriba, un tipo de datos a la izquierda y una entrada en el centro; el formulario está a la derecha. Esta es la espada de hierro de Classic RPG Rules en RPG Plus:

![El Editor de datos: mod classic_rpg_pack, a la izquierda los tipos (rpg_tileset 10, Stats 11, Archetypes 3, Equipment slots 3, Items 20, Characters 5, Battle actions 7, Shops 1, Crafting station 1, Recipe 3), en el centro sus nueve objetos con Iron sword seleccionado, y a la derecha el formulario: Id, Name y Description con un texto en cada uno de los siete idiomas, Cost 30, Picture, Item types TargetUsed, Item action action/equip, Equip type sword y un rasgo sobre el atributo Attack.](assets/data-editor-item.jpg)

La imagen del editor es de una captura anterior: las claves `rpg_demo/` o la paleta `rpg_tileset` que aparecen son históricas. Usa las claves actuales `rpg_plus/` y el modelo `rpg_object` descritos en el texto.

El formulario reúne los campos de varios mods en uno: Id, nombre, descripción, coste, imagen, tipos de objeto y acción de objeto los describe Items Engine; **Equip type**, la lista **Traits** (un atributo y una cantidad) y las clases que pueden llevarlo los añade Equipment. Un tipo que describen dos mods sigue siendo un solo formulario.

- Los **textos** (`Name`, `Description`) se muestran como su clave de traducción y una línea por idioma del mod; se escriben en los archivos de localización del propio mod.
- Las **referencias** (un atributo, un objeto, una clase) son listas de los id que conoce el juego.
- Las **listas** como traits tienen una tarjeta por elemento con sus propios campos.
- Las propiedades que ningún esquema describe se conservan y se muestran entre otros campos como JSON.

La barra: **Nueva** (Ctrl+N) añade una entrada con los valores por defecto del esquema, **Copia** (Ctrl+D) duplica la elegida, **Borrar** pregunta antes, **Deshacer**/**Rehacer** (Ctrl+Z / Ctrl+Y o Ctrl+Shift+Z) recorren los cambios, **Guardar** (Ctrl+S) escribe solo los archivos modificados, con saltos de línea LF. Justo después de guardar, el juego en marcha ve el cambio: un objeto nuevo se puede dar y equipar sin reiniciar.

En la lista aparecen las entradas del mod elegido y, si se marca **Entries of other mods**, las de sus dependencias, marcadas `↗ dueño`. Editar una de ellas escribe un parche en el mod elegido (`patches/<tipo>/<mod>.json`) solo con los valores cambiados; el archivo del dueño queda idéntico byte a byte. **Reset to inherited** quita el parche, y **↺** junto a un campo quita su valor.

## La paleta de comandos

Elige una acción o un comando a la izquierda (busca por palabras como «daño», «objeto», «check»), rellena el formulario y la línea aparece debajo:

![La paleta con Change a stat for good seleccionado: «Descrito por el mod stats_base», Stat Strength · str, Value 2, Who vacío, la línea «action stat.add stat=str value=2» y «La línea está bien».](assets/palette-line.jpg)

La línea se comprueba mientras escribes, con las mismas reglas que el Validator. Un atributo mal escrito:

![El mismo formulario con el atributo strenght: la línea «action stat.add stat=strenght value=2» y el error «'stat.add': 'stat' nombra el atributo 'strenght', que ningún mod cargado define».](assets/palette-misspelt.jpg)

**Insertar** coloca la línea en el escenario abierto en el editor de escenarios, bajo el cursor; **Copiar** la pone en el portapapeles. El escenario de la vidente de RPG Plus tras insertar:

![El editor de escenarios con seer.scenario marcado como modificado (seer *): la línea 2 es ahora «action stat.add stat=str value=2», entre el saludo de la vidente y la prueba de suerte.](assets/scenario-inserted.jpg)

Un campo `flag` tiene una casilla **self.**: marcada, la línea escribe el flag del objeto cuyo diálogo se ejecuta (`flag self.picked 1`). Un `self.` sin nombre se muestra bajo la línea como error; **Insertar** no se detiene por ello, así que corrija antes el nombre.

No se escribe nada en disco hasta que el autor pulse **Guardar** en el editor de escenarios. El texto del escenario sigue siendo la fuente de la verdad: la paleta solo ayuda a escribirlo.

## Si algo sale mal

1. **Falta un tipo o una acción.** Ningún mod cargado la describe en un esquema; la página de Schemas explica cómo añadir una.
2. **La lista de mods está vacía.** No hay ningún mod en el que escribir (solo se puede en mods instalados como carpetas; los empaquetados no).
3. **Insertar dice que no hay ningún escenario abierto.** Abre uno antes en el editor de escenarios o usa Copiar.
4. **Un campo es una caja de texto simple.** Su tipo de esquema es text o desconocido.

## Campos adicionales

`quests_base`, `skills_base` y `turn_battle_base` describen misiones, habilidades y estados. Los pasos de misión se editan como número y texto por idioma; tipos y acciones usan selectores. El campo de flag permite **self.**: `flag self.picked 1`. Un `self.` sin nombre es inválido; corrígelo antes de insertar.
