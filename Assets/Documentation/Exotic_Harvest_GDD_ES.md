# Exotic Harvest – Documento de Diseño de Juego (DDG)

*(también conocido como Enchanted Harvest / Jungle Harvest / Amazon Harvest)*

## Índice

1. [Resumen](#resumen)
2. [Experiencia Visual e Integración con el Escritorio](#experiencia-visual--integracion-con-el-escritorio)
3. [Sistemas de Interfaz de Usuario](#sistemas-de-interfaz-de-usuario)
4. [Sistemas de Economía y Recursos](#sistemas-de-economía--recursos)
5. [Sistemas de Mascotas](#sistemas-de-mascotas)
6. [Sistemas de Tiempo y Clima](#sistemas-de-tiempo--clima)
7. [Sistemas de Inventario y Tienda](#sistemas-de-inventario--tienda)
8. [Sistemas de Progresión y Mantenimiento](#sistemas-de-progresión--mantenimiento)
9. [Sistemas de Mapa y Exploración](#sistemas-de-mapa--exploración)
10. [Audio Sistemas](#audio-systems)
11. [Sistemas Multiplataforma](#cross-platform-systems)
12. [Sistemas de Eventos Especiales](#special-event-systems)

---

## Resumen

### Concepto Principal

Un tranquilo juego de escritorio para acompañar al jugador, desarrollado en Unity, que se ejecuta con un fondo transparente para que el escritorio de Windows/Mac permanezca visible. El juego combina dos componentes principales:

**Entorno Visual Acogedor**
Objetos decorativos con temática selvática, fauna ambiental, efectos diurnos, nocturnos y meteorológicos, mascotas y un entorno arrastrable y configurable que se integra en el espacio de trabajo del usuario.

**Jugabilidad de Recolección de Recursos**
Una combinación de búsqueda activa de alimentos y recolección pasiva inactiva, que impulsa una economía que se utiliza para desbloquear nuevas decoraciones, herramientas, mascotas y mejoras.

La estética de fantasía y acogedora incluye flora selvática, objetos tiki, fauna, recursos mágicos del cielo nocturno y eventos raros y extravagantes (sirena, unicornio, etc.).

### Estrategia de Monetización
- **Precio objetivo en Steam**: Mínimo 8 $
- **Compras dentro de la aplicación (IAP) opcionales** (moneda fuerte)
- **Anuncios opcionales para mejoras** (la versión para PC puede desactivar los anuncios)

---

## Experiencia Visual e Integración con el Escritorio

### Integración con el Escritorio
- **Fondo transparente** permite al jugador ver su escritorio real.
- **Decoraciones arrastrables** decoran las esquinas de la pantalla, pero evitan bloquear áreas importantes.
- **Fauna en movimiento**: serpientes que se deslizan ocasionalmente, pájaros que vuelan hacia sus nidos, luciérnagas que brillan, orugas que se convierten en capullos → mariposas.
- **Fondo de cielo** con alfa ajustable (totalmente transparente → totalmente visible).
- **Ciclos climáticos**: lluvia, tormentas, nieve, viento, luciérnagas nocturnas.
- **Ciclo lento día-noche** con amanecer, atardecer, fases lunares y nubes visibles.

### Decoraciones (Objetos del Mundo Arrastrables)
Plantas de la selva, plantas de interior, campanas de viento de bambú, máscaras, tiki. Antorchas, lámparas, enredaderas, fuentes, perchas para pájaros, buzones, cabañas, terrarios, etc.

**Características de la decoración:**
- Colocación al arrastrar
- Posición de bloqueo/desbloqueo
- Algunas pueden animarse o tener efectos pasivos (las ramas dan frutos, etc.)

---

## Sistemas de interfaz de usuario

### Componentes principales de la interfaz de usuario
- **Pestañas del menú lateral**: Ajustes, Inventario, Tienda, Mascotas, Mapas, Artesanía, Mejoras
- **Ventanas de interfaz de usuario arrastrables**: movibles y acoplables
- **Notificaciones**: recurso obtenido, evento de mascota, inventario lleno, etc.

### Sistemas de interacción
- **Botón Minimizar/Maximizar**: con atajos de teclado o clic/desplazamiento del ratón
- **Esquinas activas**: activadores opcionales para minimizar/maximizar
- **Activar el modo de arrastre**: permite reorganizar las decoraciones; Indicador de IU claro

### Información visual
- Indicadores visuales claros para la activación del modo de arrastre
- Notificaciones de avisos para todos los eventos importantes
- Elementos de IU responsivos que se adaptan a la configuración de transparencia

---

## Economía y sistemas de recursos

### Flujo económico
**Recursos → Artesanía → Mejoras → Más recursos**

### Categorías de recursos

#### Recursos primarios
- **Agua**: gotas de lluvia, rocío
- **Insectos**: orugas, mariposas, libélulas, abejas, grillos, luciérnagas, mariquitas
- **Naturaleza**: semillas, tréboles (incluidos los de 4 hojas), nueces, bayas, plumas, conchas, savia de árbol, néctar, polen
- **Cielo nocturno**: rayos de luna, polvo de estrellas, cometas, estrellas fugaces, planetas

#### Recursos valiosos
- **Objetos valiosos**: gemas, oro, joyas, reliquias raras
- **Abstracto**: secretos, sombras, recuerdos, nanas

#### Recursos de Eventos Especiales
- **Eventos Especiales**: bendición de unicornio, canto de sirena (raro; se reinicia una vez al mes)

### Sistemas de Adquisición

#### Búsqueda (Activa)
El jugador interactúa directamente con los elementos de la pantalla:
- Haz clic en hojas, rocas y gotas de rocío para recolectarlas.
- Desliza mariposas, luciérnagas y libélulas con una red.
- Arrastra arbustos para revelar objetos ocultos.
- Mantén pulsado el botón para extraer rocas o excavar tierra.
- Arrastra un cubo por la pantalla para atrapar gotas de lluvia.
- Haz clic en las flores para enviar abejas a polinizar.
- Usa herramientas (jarra, telescopio o cristal lunar) para capturar polvo de estrellas, rayos de luna y relámpagos.

*La búsqueda acelera ligeramente algunos sistemas pasivos ("Efecto de Clic de Galleta").*

#### Cosecha (Pasiva/Inactiva)
Las estructuras y mascotas colocadas generan recursos con el tiempo:
- Los **cubos** generan agua cuando Lluvias
- **Macetas**: Cultivan semillas → Plantas → Frutos
- **Colmenas**: Envían abejas a recolectar polen/miel
- **Mascotas**: Recolectan sus recursos especializados
- **Telarañas**: Atrapan luciérnagas y moscas
- **Pararrayos**: Recolectan la energía de los rayos
- **Cristales lunares**: Se cargan por la noche/fases lunares

**Propiedades de la herramienta:**
- Vida útil limitada
- Capacidad máxima (debe vaciarse manualmente)

---

## Sistemas de Mascotas

### Tipos y Comportamientos de Mascotas
Criaturas visibles que viven en la pantalla e interactúan con el entorno y el jugador:

- **Camaleón/Rana**: atrapa insectos con su lengua larga; Animaciones reactivas
- **Ardilla**: Recolecta nueces
- **Colonia de abejas**: Produce polen/miel
- **Araña**: Crea trampas de telaraña
- **Oruga → Capullo → Mariposa**: Ciclo de vida natural
- **Pez/Pulpo**: En terrario/pecera de escritorio

### Interacciones con mascotas
- **Sigue al ratón** con la mirada
- **Responde a las caricias**
- **Tienen cambios de humor** (hambrientos, juguetones, somnolientos)
- **Requieren mantenimiento ligero** (alimentación, limpieza de hábitats)

### Progresión de mascotas
- Las mascotas se pueden mejorar para una mejor recolección de recursos
- Diferentes mascotas se especializan en diferentes tipos de recursos
- La felicidad de la mascota afecta la eficiencia de la recolección

---

## Sistemas de tiempo y clima

### Jugabilidad basada en el tiempo
El tiempo influye en la apariencia, la disponibilidad de recursos y la velocidad de generación de pasivos:

- **Mañana**: Gotas de rocío, pájaros madrugadores, néctar Explosiones
- **Día**: semillas, bayas, insectos, ostras, conchas, estrellas de mar
- **Atardecer**: luciérnagas, polen al atardecer
- **Noche**: rayos de luna, polvo de estrellas, estrellas fugaces
- **Luna llena**: eventos especiales raros (cristales de carga)
- **Tormentas**: aumento de agua, relámpagos, objetos raros
- **Estacional/Mensual**: criaturas especiales, conchas raras, eventos únicos

### Sistemas Diarios/Semanales
- **Tarea Diaria, Desafío, Recompensa**
- **Bonificaciones por racha Diaria/Semanal**
- **Puzzles, objetivos, ofertas/bonificaciones específicos del día**

---

## Sistemas de Inventario y Tienda

### Estructura de la Tienda
Tienda multipágina a pantalla completa con pestañas:
- **Decoraciones**
- **Mascotas**
- **Recursos** (comprar los tipos que faltan)
- **Herramientas/Cosechadoras**
- **Potenciadores** (velocidad, Temporizadores)
- **Expansiones de inventario**
- **Protección de rachas**
- **Divisas**
- **Anuncios y ofertas**

### Gestión de inventario
- **Espacio limitado**, mejorable
- **Almacena recursos**, objetos fabricados, herramientas y decoraciones
- **La fabricación** utiliza patrones de coste similares a los de Catán (combinaciones de varios tipos de recursos)

---

## Sistemas de progresión y mantenimiento

### Bucle de mantenimiento
Los jugadores se registran regularmente para:
- Regar las plantas
- Alimentar a las mascotas
- Vaciar las herramientas llenas (cubos, panales)
- Recolectar de los árboles
- Gestionar el inventario
- Revisar el buzón en busca de regalos/misiones

### Mejoras
Los objetos mejorables incluyen:
- **Colmenas**
- **Flores/jardines**
- **Herramientas de cosecha**
- **Eficiencia de las herramientas** (cubos, redes, telescopios, palas)
- **Decoraciones**
- **Mascotas**

*Los costos usan combinaciones de múltiples recursos y escalan gradualmente.*

### Gestión del espacio de tierra/pantalla
Un área limitada significa que los jugadores deben decidir:
- Qué cosechadoras colocar
- Cuántos cubos, plantas en macetas o cristales
- Si la pantalla se vuelve visualmente recargada o se optimiza el uso de recursos

---

## Sistemas de mapas y exploración

### Tipos de mapas
Los jugadores desbloquean o encuentran mapas para nuevas "zonas", cada una con recursos y elementos visuales únicos:

- **Praderas**: tréboles, mariquitas, abejas, flores
- **Reino del cielo**: estrellas, planetas, cometas, rayos de luna
- **Playa**: conchas, ostras, perlas, dólares de arena, monedas del tesoro, mensajes en botellas

### Implementación de mapas
Los mapas a veces están ocultos dentro de otros mapas y pueden ser:
- **Minisuperposiciones**
- **Zonas de pantalla completa**
- **Montados en el escritorio "portales"**

---

## Sistemas de audio

### Diseño de audio
- **Banda sonora ambiental relajante**
- **Capas dinámicas** que cambian según la hora del día
- **Opción para silenciar la aplicación cuando está oculta**
- **Efectos de sonido de fondo**: grillos, lluvia, campanillas de viento, pájaros, ranas, agua burbujeante

---

## Sistemas multiplataforma

### Aplicación móvil complementaria
Una aplicación móvil permite:
- Cosechar
- Comprar objetos
- Mejorar herramientas
- Gestionar el inventario
- Completar tareas diarias

*Se sincroniza con el PC para que el progreso se transfiera.*

---

## Sistemas de eventos especiales

### Eventos raros
Los encuentros mágicos ocasionales otorgan objetos raros de un solo uso:
- **Sirena** aparece en un mapa de playa
- **Unicornio** en un prado durante la luna llena
- **Pluma de fénix** de un meteorito raro
- **Espíritu ancestral de la jungla** cae encantado Objetos

*Tras descubrir uno, un mes de espera antes de otro encuentro.*

---

## Notas Técnicas

### Requisitos del Prototipo
Un primer prototipo sencillo debe incluir:
- Ventana con fondo transparente
- Decoración arrastrable
- Un recolector pasivo (p. ej., un cubo que se llena al activarse la lluvia)
- Una mecánica de recolección activa (hacer clic para recoger rocío)
- Ciclo básico de día/noche y clima
- Inventario y marcador de posición de tienda sencillos
- Interfaz de usuario minimalista (mostrar/ocultar, modo de arrastre, notificaciones emergentes)

### Áreas de Desarrollo Futuro
- Maquetas visuales y flujo de interfaz de usuario
- Creación del registro de desarrollo
- Desarrollo de la propuesta de Kickstarter
- Ajustes de monetización
- Página de Steam y tráiler

---

*Última actualización: 30 de noviembre de 2025*