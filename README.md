# PilaRepo — Estructura de datos Pila (C# / .NET 8)

Repositorio con **dos proyectos que comparten una sola copia del código** (`Shared/`):

```
PilaRepo/
├── PilaRepo.sln
├── Shared/              Node<T>.cs, Pila<T>.cs, Libro.cs, Contacto.cs, PilaContactos.cs
├── PilaWinForms_Tabs/   Windows Forms (net8.0-windows), TabControl de 5 pestañas
└── PilaConsoleApp/      Consola (net8.0), dos menús
```

Ambos `.csproj` incluyen `Shared/` con `<Compile Include="..\Shared\*.cs" />`.
Todas las clases son `internal` (evita CS0050/CS0053).

## Compilar y ejecutar

```
dotnet build PilaRepo.sln
dotnet run --project PilaConsoleApp
dotnet run --project PilaWinForms_Tabs      # solo en Windows
```
(En Visual Studio 2022: abrir `PilaRepo.sln`.) La ventana WinForms requiere Windows.

## Modelo de datos

| Clase | Descripción |
|---|---|
| `Node<T>` | Dato `Data` + enlace `Prev` al nodo de abajo (null en la base). Sin constructor vacío. |
| `Libro` | `Titulo`, `Autor`. `Equals`/`GetHashCode` por `Titulo`. |
| `Contacto` | `Nombre`, `Telefono`, `Direccion`, `Correo`. `Equals`/`GetHashCode` por `Nombre`. |
| `Pila<T>` | 3 modos: **Estática** `new Pila<T>(tamaño)`, **Dinámica** `new Pila<T>()`, **Nativa** `Pila<T>.CrearNativa()`. |
| `PilaContactos` | Pila sobre `List<Contacto>` (Add / RemoveAt del último índice), precargada con 3 contactos. |

API (como `Stack<T>`): `Push`, `Pop`, `Peek`, `Clear`, `Contains` y `Count` como **propiedad**. No existe `IsEmpty`.

Errores (`InvalidOperationException`):
* Push en estática llena → `Stack Overflow (la pila está llena)`
* Pop/Peek en pila vacía → `Stack Underflow (la pila está vacía)`

`Contains` copia la pila con `ToList()` y recorre la copia con el cursor `current` de la cima a la base;
nunca toca la pila original ni su `top`.

## Datos de demo
* Estática: `Pila<int>` tamaño 4 (índices 0-3), vacía, `top = -1`. El 5.º `Push` produce Overflow.
* Nativa: `Pila<int>` vacía.
* Dinámica: `Pila<Libro>` precargada (El Principito, 1984, Cien Años de Soledad = tope).
  La pestaña trae prellenado "Don Quijote de la Mancha / Miguel de Cervantes" para el Push de la demo.
  La precarga está en `Libro.CrearPilaPrecargada()` (una sola copia para WinForms y consola).
* Contactos: 3 contactos de ejemplo (Ana López, Luis Pérez, María García).

## Interfaz WinForms (5 pestañas)
1. Pila Estática (NumericUpDown de tamaño + "Crear / Reiniciar Pila", muestra `top = N`)
2. Pila Dinámica (libros)
3. Pila Nativa (`Stack<T>`)
4. Pila de Contactos (`List<T>`)
5. Comparación (tres `Pila<int>` propias de la pestaña; "Push/Pop/Clear en las 3" indican cuál falló y por qué)

Un botón por operación; cada uno refresca la grilla y muestra en la etiqueta de estado la acción realizada
(o el error en rojo). Las grillas crean sus columnas una vez y llenan con `Rows.Add` (sin `DataSource`);
el tope se marca con `Tope ->`. Al cambiar de pestaña se refresca la grilla visible.

## Consola
Menú 1: Estática (pide tamaño) / Dinámica / Nativa / Contactos / Salir.
Menú 2: Push, Pop, Peek, Count, Clear, Contains, Ver pila completa, Cambiar de modo, Salir.
`InvalidOperationException` se captura y se muestra su mensaje.

## Fidelidad con la presentación
* `Count` dinámico **recorre los nodos** con el cursor `current` desde `top` hasta `null` (diapositivas "Listas enlazadas count"); no hay contador guardado.
* `Count` estático es `top + 1`. Se dejó como **propiedad** (`public int Count => top + 1`); la diapositiva 100 lo muestra como método `Count()`: conviene cambiar esa diapositiva.
* Push/Pop/Peek/Clear/Contains siguen las animaciones: el tope sube/baja, `Prev` enlaza al nodo de abajo, `Contains` baja con `current` de la cima a la base.
