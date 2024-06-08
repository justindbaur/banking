import { InjectionToken, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { RegisterComponent } from './register/register.component';
import { ReactiveFormsModule } from '@angular/forms';
import { NavbarComponent } from './navbar/navbar.component';
import { LayoutModule } from '@angular/cdk/layout';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatCardModule } from '@angular/material/card';
import {
  MAT_FORM_FIELD_DEFAULT_OPTIONS,
  MatFormFieldModule,
} from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { LoginComponent } from './login/login.component';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { Client } from '@passwordlessdev/passwordless-client';
import { ApiService } from './services/api.service';
import { CreateComponent } from './api-keys/create/create.component';
import { environment } from '../environments/environment';
import { UserService } from './services/user.service';
import { HomeComponent } from './home/home.component';
import { ListComponent } from './api-keys/list/list.component';
import { ApiKeysComponent } from './api-keys/api-keys.component';

export const BASE_URL = new InjectionToken<string>('BASE_URL');
const PASSWORDLESS_API_URL = new InjectionToken<string>('PASSWORDLESS_API_URL');
const PASSWORDLESS_API_KEY = new InjectionToken<string>('PASSWORDLESS_API_KEY');

@NgModule({
  declarations: [
    AppComponent,
    RegisterComponent,
    NavbarComponent,
    LoginComponent,
    CreateComponent,
    HomeComponent,
    ListComponent,
    ApiKeysComponent,
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    LayoutModule,
    MatToolbarModule,
    MatButtonModule,
    MatSidenavModule,
    MatIconModule,
    MatListModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatSelectModule,
  ],
  providers: [
    {
      provide: MAT_FORM_FIELD_DEFAULT_OPTIONS,
      useValue: { appearance: 'fill' },
    },
    { provide: BASE_URL, useValue: environment.apiUrl },
    { provide: PASSWORDLESS_API_KEY, useValue: environment.passwordlessApiKey },
    {
      provide: PASSWORDLESS_API_URL,
      useValue: environment.passwordlessApiUrl ?? 'https://v4.passwordless.dev',
    },
    {
      provide: Client,
      useFactory: (apiUrl: string, apiKey: string) =>
        new Client({ apiUrl, apiKey }),
      deps: [PASSWORDLESS_API_URL, PASSWORDLESS_API_KEY],
    },
    {
      provide: ApiService,
    },
    provideHttpClient(withInterceptors([])),
    UserService,
  ],
  bootstrap: [AppComponent],
})
export class AppModule {}
