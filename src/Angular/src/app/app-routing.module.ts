import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { CreateTokenComponent } from './create-token/create-token.component';
import { LoginComponent } from './login/login.component';
import { ApiKeysComponent } from './api-keys/api-keys.component';
import { CreateApiKeyComponent } from './create-api-key/create-api-key.component';

const routes: Routes = [
  { path: 'register', component: CreateTokenComponent },
  { path: 'login', component: LoginComponent },
  { path: 'api-keys', component: ApiKeysComponent },
  { path: 'api-keys/create', component: CreateApiKeyComponent },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
