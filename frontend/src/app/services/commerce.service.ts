
import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';

export interface CommerceQuarantine {
  id: number;
  pc_processdate: string | null;
  pc_nomcomred: string | null;
  pc_numdoc: string | null;
  motivo: string | null;
}

export interface UploadResponse {
  mensaje: string;
  registrosInsertados: number;
}

export interface ProcessResponse {
  mensaje: string;
  fecha: string;
  quarantinedCount: number;
}

export interface CommerceItem {
  id: number;
  pc_processdate: string | null;
  pc_nomcomred: string | null;
  pc_numdoc: string | null;
}


@Injectable({
  providedIn: 'root'
})
export class CommerceService {

  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:7204/api/commerce';

  uploadCsv(archivo: File) {
    const formData = new FormData();
    formData.append('file', archivo);

    return this.http.post<UploadResponse>(
      `${this.apiUrl}/upload`,
      formData
    );
  }

  processCommerce(fecha: string) {
    return this.http.post<ProcessResponse>(
      `${this.apiUrl}/process`,
      { processDate: fecha }
    );
  }

  getQuarantine() {
    return this.http.get<CommerceQuarantine[]>(
      `${this.apiUrl}/quarantine`
    );
  }
  
getCommerce(fecha?: string) {
  return this.http.get<CommerceItem[]>(
    this.apiUrl,
    {
      params: fecha ? { fecha } : {}
    }
  );
}

}

