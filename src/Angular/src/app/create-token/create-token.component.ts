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

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private client: Client,
    private apiService: ApiService) {
  }

  public register() {
    this.apiService.register({
      nickname: this.form.value.nickname!,
      username: this.form.value.username!,
    })
      .subscribe(r => {
        const { token } = r;
        this.continueLogin(token);
      });
  }

  private async continueLogin(token: string) {
    try {
      await this.client.register(token, this.form.value.nickname!);
      await this.router.navigate(['/login']);
    } catch (e) {
      console.error(e);
    }
  }
}
