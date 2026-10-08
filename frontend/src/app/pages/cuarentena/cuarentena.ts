
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  CommerceService,
  CommerceQuarantine
} from '../../services/commerce.service';

@Component({
  selector: 'app-cuarentena',
  imports: [],
  templateUrl: './cuarentena.html'
})
export class Cuarentena implements OnInit {

  private readonly commerceService = inject(CommerceService);

  registros = signal<CommerceQuarantine[]>([]);
  cargando = signal(false);
  error = signal('');
  filtroFecha = signal('');

  registrosFiltrados = computed(() => {
    const fecha = this.filtroFecha();

    if (!fecha) return this.registros();

    return this.registros().filter(
      registro => registro.pc_processdate === fecha
    );
  });

  ngOnInit(): void {
    this.cargarRegistros();
  }

  cargarRegistros(): void {
    this.cargando.set(true);
    this.error.set('');

    this.commerceService.getQuarantine().subscribe({
      next: datos => {
        this.registros.set(datos);
        this.cargando.set(false);
      },

      error: err => {
        console.error(err);
        this.error.set('No se pudieron consultar los registros.');
        this.cargando.set(false);
      }
    });
  }

  actualizarFiltro(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.filtroFecha.set(input.value);
  }

  limpiarFiltro(): void {
    this.filtroFecha.set('');
  }
}
