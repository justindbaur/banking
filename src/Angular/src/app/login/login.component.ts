import { Component } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Client } from '@passwordlessdev/passwordless-client';
import { ApiService } from '../services/api.service';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  form = this.fb.nonNullable.group({
    username: new FormControl('', Validators.required),
  });

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private apiService: ApiService,
    private client: Client
  ) {}

  public async login() {
    const tokenResponse = await this.client.signinWithAlias(
      this.form.value.username!
    );

    if (tokenResponse.error != null) {
      console.log(tokenResponse.error);
      return;
    }

    await lastValueFrom(this.apiService.login$(tokenResponse.token));

    await this.router.navigate(['/api-keys']);
  }
}
