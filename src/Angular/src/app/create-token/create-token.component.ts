import { Component } from '@angular/core';
import { FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { Client } from "@passwordlessdev/passwordless-client";
import { ApiService } from '../services/api.service';

@Component({
  selector: 'app-create-token',
  templateUrl: './create-token.component.html',
  styleUrls: ['./create-token.component.scss']
})
export class CreateTokenComponent {
  form = this.fb.nonNullable.group({
    fullName: new FormControl('', Validators.required),
    nickname: new FormControl('', Validators.required),
    username: new FormControl('', Validators.required),
  });
  private baseUrl = "http://localhost:5026";

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private client: Client,
    private apiService: ApiService) {

  }

  public async register() {
    const token = await this.apiService.register({
      fullName: this.form.value.fullName!,
      nickname: this.form.value.nickname!,
      username: this.form.value.username!,
    });
    try {
      await this.client.register(token, this.form.value.nickname!);
      await this.router.navigate(['/login']);
    } catch (e) {
      console.error(e);
    }
  }
}
