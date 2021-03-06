import { Component, Output, EventEmitter, Input, ViewChild, ElementRef } from '@angular/core';
import { UploadDownloadService } from 'src/app/services/upload-download.service';
import { HttpEventType } from '@angular/common/http';
import { ProgressStatus, ProgressStatusEnum } from 'src/app/models/progress-status.model';

@Component({
  selector: 'app-upload',
  templateUrl: 'upload.component.html'
})

export class UploadComponent {
  @Input() public disabled: boolean;
  @Output() public uploadStatus: EventEmitter<ProgressStatus>;
  @ViewChild('inputFile') inputFile: ElementRef;

  constructor(private service: UploadDownloadService) {
    this.uploadStatus = new EventEmitter<ProgressStatus>();
  }

  public upload(event) {

    if (event.target.files && event.target.files.length > 0) {
      for (let index = 0; index < event.target.files.length; index++) {
        //let file = event.target.files[index];
        const file = event.target.files[index];
        this.uploadStatus.emit( {status: ProgressStatusEnum.START});
        this.service.uploadFile(file).subscribe(
          data => {
            if (data) {
              switch (data.type) {
                case HttpEventType.UploadProgress:
                  this.uploadStatus.emit( {status: ProgressStatusEnum.IN_PROGRESS, percentage: Math.round((data.loaded / data.total) * 100)});
                  break;
                case HttpEventType.Response:
                  this.inputFile.nativeElement.value = '';
                  this.uploadStatus.emit( {status: ProgressStatusEnum.COMPLETE});
                  break;
              }
            }
          },
          error => {
            this.inputFile.nativeElement.value = '';
            if (error.message == 'Http failure response for http://localhost:5001/api/upload: 415 Unsupported Media Type')
              this.uploadStatus.emit( {status: ProgressStatusEnum.NOT_VALID_TYPE});
            else if (error.message == 'Http failure response for http://localhost:5001/api/upload: 400 Bad Request')
              this.uploadStatus.emit( {status: ProgressStatusEnum.NOT_VALID_SIZE});
            else
              this.uploadStatus.emit( {status: ProgressStatusEnum.ERROR});
          }
        );
      }

    }
  }
}
