export interface ProgressStatus {
  status: ProgressStatusEnum;
  percentage?: number;
}

export enum ProgressStatusEnum {
  START, COMPLETE, IN_PROGRESS, ERROR, NOT_VALID_TYPE, NOT_VALID_SIZE
}
