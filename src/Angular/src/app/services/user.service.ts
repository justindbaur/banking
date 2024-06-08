import {
  Observable,
  Subject,
  concatMap,
  defer,
  map,
  merge,
  of,
  shareReplay,
  switchMap,
  tap,
} from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { BASE_URL } from '../app.module';
import { Client } from '@passwordlessdev/passwordless-client';

export type UserInfo = {
  isAuthenticated: boolean;
};

@Injectable()
export class UserService {
  private readonly userInfo = new Subject<UserInfo>();
  userInfo$: Observable<UserInfo>;

  constructor(
    private readonly httpClient: HttpClient,
    private readonly passwordlessClient: Client,
    @Inject(BASE_URL) private readonly baseUrl: string
  ) {
    this.userInfo$ = merge(
      defer(() => this.getInfo$()),
      this.userInfo.asObservable()
    ).pipe(shareReplay(1));
  }

  login$(username: string) {
    return defer(
      async () => await this.passwordlessClient.signinWithAlias(username)
    ).pipe(
      switchMap((tokenResponse) => {
        if (tokenResponse.error) {
          console.log(tokenResponse.error);
          throw new Error(tokenResponse.error.detail);
        }

        return of(tokenResponse.token);
      }),
      switchMap((token) =>
        this.httpClient.post<void>(
          `${this.baseUrl}/passwordless-login`,
          {
            token,
          },
          {
            withCredentials: true,
          }
        )
      )
    );
  }

  register$(registerRequest: { username: string; nickname: string }) {
    return this.httpClient
      .post<{ token: string }>(
        `${this.baseUrl}/passwordless-register`,
        registerRequest
      )
      .pipe(
        concatMap(async (response) => {
          return await this.passwordlessClient.register(
            response.token,
            registerRequest.nickname
          );
        }),
        map((response) => {
          if (response.error) {
            throw new Error(response.error.detail);
          }

          return response.token;
        })
      );
  }

  refreshInfo$() {
    return this.getInfo$().pipe(tap((info) => this.userInfo.next(info)));
  }

  private getInfo$(): Observable<UserInfo> {
    return this.httpClient.get<UserInfo>(`${this.baseUrl}/user/info`, {
      withCredentials: true,
    });
  }
}
