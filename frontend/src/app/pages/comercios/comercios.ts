
import { Component, OnInit, inject, signal } from '@angular/core';
import {
  CommerceService,
  CommerceItem
} from '../../services/commerce.service';

@Component({
  selector: 'app-comercios',
  imports: [],
  templateUrl: './comercios.html'
})
export class Comercios implements OnInit {

  private readonly commerceService = inject(CommerceService);

  registros = signal<CommerceItem[]>([]);
  fecha = signal('');
  cargando = signal(false);
  error = signal('');

  ngOnInit(): void {
    this.cargarRegistros();
  }

  cargarRegistros(): void {
    this.cargando.set(true);
    this.error.set('');

    this.commerceService.getCommerce(this.fecha()).subscribe({
      next: respuesta => {
        this.registros.set(respuesta);
        this.cargando.set(false);
      },
      error: err => {
        console.error(err);
        this.error.set('No se pudieron consultar los comercios.');
        this.cargando.set(false);
      }
    });
  }

  cambiarFecha(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.fecha.set(input.value);
    this.cargarRegistros();
  }

  limpiarFiltro(): void {
    this.fecha.set('');
    this.cargarRegistros();
  }
}
