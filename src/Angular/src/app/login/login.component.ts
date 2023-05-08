import { Component } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Client } from '@passwordlessdev/passwordless-client';
import { ApiService } from '../services/api.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  form = this.fb.nonNullable.group({
    username: new FormControl('', Validators.required),
  });

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private apiService: ApiService,
    private client: Client) {
  }

  public async login() {
    const token = await this.client.signinWithAlias(this.form.value.username!);
    console.log(token);

    await this.apiService.login(token);

    this.router.navigate(['/api-keys']);
  }
}
