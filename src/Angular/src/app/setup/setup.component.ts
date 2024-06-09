import { Component } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-setup',
  standalone: true,
  imports: [MatCardModule, MatButtonModule, RouterLink, RouterOutlet],
  templateUrl: './setup.component.html',
})
export class SetupComponent {}
