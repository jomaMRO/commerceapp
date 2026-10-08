
import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'importar',
    pathMatch: 'full'
  },
  {
    path: 'importar',
    loadComponent: () =>
      import('./pages/importar/importar').then(m => m.Importar)
  },
  {
    path: 'comercios',
    loadComponent: () =>
      import('./pages/comercios/comercios').then(m => m.Comercios)
  },
  {
    path: 'procesar',
    loadComponent: () =>
      import('./pages/procesar/procesar').then(m => m.Procesar)
  },
  {
    path: 'cuarentena',
    loadComponent: () =>
      import('./pages/cuarentena/cuarentena').then(m => m.Cuarentena)
  },
  {
    path: '**',
    redirectTo: 'importar'
  }
];
