import { HttpClient } from "@angular/common/http";
import { Inject, Injectable } from "@angular/core";
import { BASE_URL } from "../app.module";
import { Observable, firstValueFrom, map } from "rxjs";
import { Jsonify } from "type-fest";

export class ApiKey {
  id: string;
  name: string;
  scopes: string[];
  expirationDate?: Date;
  createdDate: Date;
  lastUsedDate?: Date;

  constructor(json: Jsonify<ApiKey>) {
    this.id = json.id;
    this.name = json.name;
    this.scopes = json.scopes;
    this.createdDate = new Date(json.createdDate);
    this.expirationDate = json.expirationDate ? new Date(json.expirationDate) : undefined;
    this.lastUsedDate = json.lastUsedDate ? new Date(json.lastUsedDate) : undefined;
  }
}

@Injectable()
export class ApiService {
  constructor(private httpClient: HttpClient, @Inject(BASE_URL) private baseUrl: string) {

  }

  getApiKeys(): Observable<ApiKey[]> {
    console.log("getting api-keys");
    return this.httpClient.get<Jsonify<ApiKey[]>>(`${this.baseUrl}/jwt`, {
      withCredentials: true
    })
      .pipe(map(j => j.map(k => new ApiKey(k))));
  }

  login(token: string) {
    return this.httpClient.post<void>(
      `${this.baseUrl}/passwordless-login`, {
      token
    }, {
      // TODO: Does this actually need credentials? only to set the cookie?
      withCredentials: true,
    });
  }

  register(loginRequest: { username: string, nickname: string }): Observable<{ token: string}> {
    return this.httpClient.post<{ token: string }>(
      `${this.baseUrl}/passwordless-register`,
      loginRequest);
  }
}
