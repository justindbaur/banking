import { Component } from '@angular/core';
import {
  SourceAccount,
  SourceListItem,
  SourcesService,
} from './sources.service';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Observable, lastValueFrom, map, shareReplay, switchMap } from 'rxjs';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatTableModule } from '@angular/material/table';

@Component({
  selector: 'app-source',
  standalone: true,
  template: `<ng-container *ngIf="source$ | async as source">
    <div class="card-container">
      <div class="split-card-container">
        <mat-card class="split-card">
          <mat-card-content>
            <p>{{ source.displayName }}</p>
          </mat-card-content>
        </mat-card>
      </div>
      <div class="split-card-container">
        <mat-card class="split-card">
          <mat-card-content>
            <p>{{ source.displayName }}</p>
          </mat-card-content>
        </mat-card>
      </div>
      <div style="margin: 2px; width: 100%;">
        <mat-card>
          <mat-card-content>
            <mat-table [dataSource]="accounts$" style="width: 100%;">
              <ng-container matColumnDef="accountId">
                <th mat-header-cell *matHeaderCellDef>Account ID</th>
                <td mat-cell *matCellDef="let element">
                  {{ element.accountId }}
                </td>
              </ng-container>

              <ng-container matColumnDef="name">
                <th mat-header-cell *matHeaderCellDef>Name</th>
                <td mat-cell *matCellDef="let element">{{ element.name }}</td>
              </ng-container>

              <ng-container matColumnDef="balance">
                <th mat-header-cell *matHeaderCellDef>Balance</th>
                <td mat-cell *matCellDef="let element">
                  {{ element.balance }}
                </td>
              </ng-container>

              <ng-container matColumnDef="enabled">
                <th mat-header-cell *matHeaderCellDef>Enabled</th>
                <td mat-cell *matCellDef="let element">
                  {{ element.enabled ? 'Yes' : 'No' }}
                </td>
              </ng-container>

              <tr mat-header-row *matHeaderRowDef="displayedColumns"></tr>
              <tr mat-row *matRowDef="let row; columns: displayedColumns"></tr>
            </mat-table>
          </mat-card-content>
        </mat-card>
      </div>
    </div>
  </ng-container>`,
  imports: [
    RouterModule,
    CommonModule,
    MatCardModule,
    MatGridListModule,
    MatTableModule,
  ],
  providers: [SourcesService],
  styles: `
      .card-container {
        display: flex;
        flex-wrap: wrap;
        align-items: space-evenly;
      }

      .split-card-container {
        flex: 0 0 50%;
      }

      .split-card {
        margin: 2px
      }
    `,
})
export class SourceComponent {
  readonly displayedColumns = ['accountId', 'name', 'balance', 'enabled'];

  source$: Observable<SourceListItem>;
  accounts$: Observable<SourceAccount[]>;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly sourcesService: SourcesService
  ) {
    const sourceId$ = this.route.paramMap.pipe(
      map((p) => p.get('id')!),
      shareReplay(1)
    );

    this.source$ = sourceId$.pipe(
      switchMap((id) => this.sourcesService.get$(id))
    );

    this.accounts$ = sourceId$.pipe(
      switchMap((id) => this.sourcesService.getAccounts$(id)),
      map((r) => r.data)
    );
  }

  async delete(sourceId: string) {
    await lastValueFrom(this.sourcesService.delete$(sourceId));
  }
}
