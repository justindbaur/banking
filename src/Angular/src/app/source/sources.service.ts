import { Inject, Injectable } from '@angular/core';
import { BASE_URL } from '../app.module';
import { HttpClient } from '@angular/common/http';
import { ListResponse } from '../services/api.service';

export type SourceListItem = {
  id: string;
  sourceId: string;
  displayName: string;
  enabled: boolean;
};

export type Source = {
  id: string;
  sourceId: string;
  displayName: string;
  enabled: boolean;
};

export type SourceAccount = {
  id: string;
  accountId: string;
  name: string;
  balance: number;
  enabled: boolean;
  extraInfo: unknown;
};

@Injectable()
export class SourcesService {
  constructor(
    @Inject(BASE_URL) private readonly baseUrl: string,
    private readonly httpClient: HttpClient
  ) {}

  get$(sourceId: string) {
    return this.httpClient.get<Source>(`${this.baseUrl}/sources/${sourceId}`, {
      withCredentials: true,
    });
  }

  getAll$() {
    return this.httpClient.get<ListResponse<SourceListItem>>(
      `${this.baseUrl}/sources`,
      { withCredentials: true }
    );
  }

  getAccounts$(sourceId: string) {
    return this.httpClient.get<ListResponse<SourceAccount>>(
      `${this.baseUrl}/sources/${sourceId}/accounts`,
      { withCredentials: true }
    );
  }

  delete$(sourceId: string) {
    return this.httpClient.delete<void>(`${this.baseUrl}/sources/${sourceId}`, {
      withCredentials: true,
    });
  }
}
