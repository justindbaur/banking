import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiKey, ApiService } from '../../services/api.service';

@Component({
  selector: 'app-list-api-keys',
  templateUrl: './list.component.html',
  styleUrls: ['./list.component.scss'],
})
export class ListComponent {
  displayedColumns = ['name', 'created', 'lastUsed', 'expiration'];
  apiKeys$: Observable<ApiKey[]>;

  constructor(private apiService: ApiService) {
    this.apiKeys$ = this.apiService.getApiKeys$();
  }
}
