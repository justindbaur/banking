import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { RegisterComponent } from './register/register.component';
import { LoginComponent } from './login/login.component';
import { CreateComponent } from './api-keys/create/create.component';
import { HomeComponent } from './home/home.component';
import { authenticated } from './guards/authenticated.guard';
import { ListComponent as ApiKeysListComponent } from './api-keys/list/list.component';
import { ApiKeysComponent } from './api-keys/api-keys.component';
import { SetupComponent } from './setup/setup.component';
import { NewComponent } from './setup/new/new.component';
import { NewItemComponent } from './setup/new-item/new-item.component';
import { SourceComponent } from './source/source.component';
import { ListComponent as SourcesListComponent } from './source/list.component';

const routes: Routes = [
  {
    path: '',
    component: HomeComponent,
    title: 'Home',
    data: { pageTitle: 'Home' },
    canActivate: [authenticated],
  },
  {
    path: 'register',
    component: RegisterComponent,
    data: { pageTitle: 'Register' },
  },
  { path: 'login', component: LoginComponent, data: { pageTitle: 'Login' } },
  {
    path: 'api-keys',
    component: ApiKeysComponent,
    data: { pageTitle: 'API Keys' },
    canActivate: [authenticated],
    children: [
      {
        path: '',
        component: ApiKeysListComponent,
        data: { pageTitle: 'Something' },
      },
      {
        path: 'create',
        component: CreateComponent,
        data: { pageTitle: 'Create API Key' },
      },
    ],
  },
  {
    path: 'setup',
    component: SetupComponent,
    children: [
      {
        path: 'new',
        children: [
          {
            path: '',
            component: NewComponent,
          },
          {
            path: ':id',
            component: NewItemComponent,
          },
        ],
      },
    ],
  },
  {
    path: 'sources',
    children: [
      {
        path: '',
        component: SourcesListComponent,
      },
      {
        path: ':id',
        component: SourceComponent,
      },
    ],
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
