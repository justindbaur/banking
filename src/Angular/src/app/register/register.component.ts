import { Component } from '@angular/core';
import { FormBuilder, FormControl, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { lastValueFrom } from 'rxjs';
import { UserService } from '../services/user.service';

@Component({
  selector: 'app-create-token',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
})
export class RegisterComponent {
  form = this.formBuilder.nonNullable.group({
    nickname: new FormControl('', Validators.required),
    username: new FormControl('', Validators.required),
  });

  constructor(
    private readonly formBuilder: FormBuilder,
    private readonly router: Router,
    private readonly userService: UserService
  ) {}

  public async register() {
    if (this.form.invalid) {
      return;
    }

    await lastValueFrom(
      this.userService.register$({
        nickname: this.form.value.nickname!,
        username: this.form.value.username!,
      })
    );

    await this.router.navigate(['/login']);
  }
}
