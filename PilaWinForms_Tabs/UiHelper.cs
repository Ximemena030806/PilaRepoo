using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace PilaWinForms_Tabs
{
    /// <summary>Implementado por los paneles que pueden volver a dibujar su grilla.</summary>
    internal interface IRefrescable
    {
        void Refrescar();
    }

    /// <summary>Funciones comunes de interfaz (botones, grillas, FlowLayoutPanel).</summary>
    internal static class UiHelper
    {
        public static Button Boton(string texto, EventHandler accion, int anchoMinimo = 100)
        {
            var b = new Button
            {
                Text = texto,
                AutoSize = true,
                MinimumSize = new Size(anchoMinimo, 30),
                Margin = new Padding(3)
            };
            b.Click += accion;
            return b;
        }

        public static FlowLayoutPanel NuevoFlow() => new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            WrapContents = true,
            Margin = new Padding(0, 2, 0, 2)
        };

        public static Label Etiqueta(string texto) => new Label
        {
            Text = texto,
            AutoSize = true,
            Margin = new Padding(6, 8, 0, 0)
        };

        /// <summary>
        /// Crea la grilla UNA sola vez, con sus columnas (NO se usa DataSource = List):
        /// "Índice", marca de tope y las columnas de datos. Las filas se llenan con Rows.Add.
        /// </summary>
        public static DataGridView CrearGrid(params string[] columnasDatos)
        {
            var g = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = SystemColors.Window
            };

            g.Columns.Add("colIndice", "Índice");
            g.Columns.Add("colTope", "");
            foreach (string nombre in columnasDatos)
                g.Columns.Add("col" + nombre, nombre);

            g.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            g.Columns[0].Width = 55;
            g.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            g.Columns[1].Width = 70;
            return g;
        }

        /// <summary>
        /// Llena la grilla con Rows.Add, del tope hacia la base. El tope se marca con "Tope ->".
        /// Si capacidadVisible &gt; 0 (pila estática) también se muestran las casillas libres,
        /// para ver el arreglo completo (índices 0..N-1).
        /// </summary>
        /// <param name="baseATope">Elementos en orden base -> tope (el último es el tope).</param>
        public static void LlenarGrid<T>(DataGridView grid, List<T> baseATope, int capacidadVisible,
                                         Func<T, string[]> aCeldas, int columnasDatos)
        {
            grid.Rows.Clear();
            int n = baseATope.Count;
            int inicio = Math.Max(capacidadVisible, n) - 1;

            for (int i = inicio; i >= 0; i--)
            {
                bool ocupado = i < n;
                bool esTope = ocupado && i == n - 1;

                var fila = new object?[columnasDatos + 2];
                fila[0] = i;
                fila[1] = esTope ? "Tope ->" : "";
                if (ocupado)
                {
                    string[] celdas = aCeldas(baseATope[i]);
                    for (int c = 0; c < columnasDatos && c < celdas.Length; c++)
                        fila[c + 2] = celdas[c];
                }

                int r = grid.Rows.Add(fila);
                if (esTope)
                {
                    grid.Rows[r].DefaultCellStyle.BackColor = Color.LightGoldenrodYellow;
                    grid.Rows[r].DefaultCellStyle.Font = new Font(grid.Font, FontStyle.Bold);
                }
            }
            grid.ClearSelection();
        }

        public static void MostrarEstado(Label lbl, string texto, bool error)
        {
            lbl.ForeColor = error ? Color.Red : Color.Black;
            lbl.Text = texto;
        }
    }
}
