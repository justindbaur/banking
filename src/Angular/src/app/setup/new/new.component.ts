import { HttpClient } from '@angular/common/http';
import { Component, Inject } from '@angular/core';
import { BASE_URL } from '../../app.module';
import { Observable, map } from 'rxjs';
import { ListResponse } from '../../services/api.service';

import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { RouterLink } from '@angular/router';

type Source = { id: string; name: string };

@Component({
  selector: 'app-new-source',
  standalone: true,
  imports: [MatTableModule, MatButtonModule, RouterLink],
  template: `<table mat-table [dataSource]="sources$">
    <ng-container matColumnDef="name">
      <th mat-header-cell *matHeaderCellDef>Name</th>
      <td mat-cell *matCellDef="let element">{{ element.name }}</td>
    </ng-container>

    <ng-container matColumnDef="action">
      <th mat-header-cell *matHeaderCellDef>&nbsp;</th>
      <td mat-cell *matCellDef="let element">
        <button
          mat-raised-button
          color="primary"
          routerLink="{{ './' + element.id }}"
        >
          Start
        </button>
      </td>
    </ng-container>

    <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
    <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
  </table>`,
})
export class NewComponent {
  displayedColumns = ['name', 'action'];

  sources$: Observable<Source[]> = this.httpClient
    .get<ListResponse<Source>>(`${this.baseUrl}/sources/template`, {
      withCredentials: true,
    })
    .pipe(map((r) => r.data));

  constructor(
    private readonly httpClient: HttpClient,
    @Inject(BASE_URL) private readonly baseUrl: string
  ) {}
}
