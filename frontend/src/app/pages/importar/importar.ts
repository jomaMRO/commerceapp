
import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import * as Papa from 'papaparse';
import { CommerceService } from '../../services/commerce.service';

interface RegistroCsv {
  pc_processdate: string;
  pc_nomcomred: string;
  pc_numdoc: string;
}

@Component({
  selector: 'app-importar',
  imports: [],
  templateUrl: './importar.html'
})
export class Importar {

  private readonly commerceService = inject(CommerceService);

  archivo = signal<File | null>(null);
  registros = signal<RegistroCsv[]>([]);
  totalRegistros = signal(0);

  cargando = signal(false);
  importado = signal(false);
  error = signal('');
  mensaje = signal('');

  seleccionarArchivo(event: Event): void {
    const input = event.target as HTMLInputElement;
    const archivo = input.files?.[0];

    this.archivo.set(null);
    this.registros.set([]);
    this.totalRegistros.set(0);
    this.error.set('');
    this.mensaje.set('');
    this.importado.set(false);

    if (!archivo) return;

    if (!/^commerce_\d{8}\.csv$/i.test(archivo.name)) {
      this.error.set('El archivo debe llamarse commerce_DDMMYYYY.csv.');
      return;
    }

    if (archivo.size === 0) {
      this.error.set('El archivo está vacío.');
      return;
    }

    Papa.parse<RegistroCsv>(archivo, {
      header: true,
      skipEmptyLines: 'greedy',
      transformHeader: encabezado =>
        encabezado.replace(/^\uFEFF/, '').trim(),

      complete: resultado => {
        const columnas = [
          'pc_processdate',
          'pc_nomcomred',
          'pc_numdoc'
        ];

        const encabezados = resultado.meta.fields ?? [];

        if (!columnas.every(col => encabezados.includes(col))) {
          this.error.set('El CSV no contiene las columnas requeridas.');
          return;
        }

        if (resultado.errors.length > 0) {
          this.error.set('El CSV contiene errores de formato.');
          return;
        }

        if (resultado.data.length === 0) {
          this.error.set('El archivo no contiene registros.');
          return;
        }

        this.archivo.set(archivo);
        this.registros.set(resultado.data.slice(0, 10));
        this.totalRegistros.set(resultado.data.length);
      },

      error: () => {
        this.error.set('No se pudo leer el archivo.');
      }
    });
  }

  importar(): void {
    const archivo = this.archivo();

    if (!archivo || this.cargando() || this.importado()) return;

    this.cargando.set(true);
    this.error.set('');
    this.mensaje.set('');

    this.commerceService.uploadCsv(archivo).subscribe({
      next: respuesta => {
        this.cargando.set(false);
        this.importado.set(true);

        this.mensaje.set(
          `Archivo importado correctamente. ` +
          `${respuesta.registrosInsertados} registros insertados.`
        );
      },

      error: (err: HttpErrorResponse) => {
        this.cargando.set(false);

        const detalle = typeof err.error === 'string'
          ? err.error
          : err.error?.mensaje;

        this.error.set(
          detalle || 'No se pudo importar el archivo.'
        );
      }
    });
  }
}
