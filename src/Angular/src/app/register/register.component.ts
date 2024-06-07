import { Component } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Client } from '@passwordlessdev/passwordless-client';
import { ApiService } from '../services/api.service';
import { lastValueFrom } from 'rxjs';

@Component({
  selector: 'app-create-token',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent {
  form = this.fb.nonNullable.group({
    nickname: new FormControl('', Validators.required),
    username: new FormControl('', Validators.required),
  });

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private client: Client,
    private apiService: ApiService
  ) {}

  public async register() {
    const token = await lastValueFrom(
      this.apiService.register$({
        nickname: this.form.value.nickname!,
        username: this.form.value.username!,
      })
    );
    try {
      await this.client.register(token, this.form.value.nickname!);
      await this.router.navigate(['/login']);
    } catch (e) {
      console.error(e);
    }
  }
}
