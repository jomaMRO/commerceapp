
import { Component, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import {
  CommerceService,
  ProcessResponse
} from '../../services/commerce.service';

@Component({
  selector: 'app-procesar',
  imports: [],
  templateUrl: './procesar.html'
})
export class Procesar {

  private readonly commerceService = inject(CommerceService);

  fecha = signal('');
  procesando = signal(false);
  resultado = signal<ProcessResponse | null>(null);
  error = signal('');

  actualizarFecha(event: Event): void {
    const input = event.target as HTMLInputElement;

    this.fecha.set(input.value);
    this.resultado.set(null);
    this.error.set('');
  }

  procesar(): void {
    if (!this.fecha() || this.procesando()) return;

    this.procesando.set(true);
    this.error.set('');
    this.resultado.set(null);

    this.commerceService.processCommerce(this.fecha()).subscribe({
      next: respuesta => {
        this.resultado.set(respuesta);
        this.procesando.set(false);
      },

      error: (err: HttpErrorResponse) => {
        this.procesando.set(false);

        this.error.set(
          err.error?.mensaje || 'No se pudieron procesar los registros.'
        );
      }
    });
  }
}
