import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      providers: [provideRouter([])]
    }).compileComponents();
  });

  it('crea la aplicación', () => {
    const fixture = TestBed.createComponent(App);
    expect(fixture.componentInstance).toBeTruthy();
  });

  it('muestra el nombre CommerceApp', () => {
    const fixture = TestBed.createComponent(App);
    fixture.detectChanges();
    const encabezado = fixture.nativeElement.querySelector('.brand strong');
    expect(encabezado?.textContent?.trim()).toBe('CommerceApp');
  });
});
